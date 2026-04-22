using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Contracts.Interfaces.Services;
using BT.Application.Extensions;
using BT.Application.Features.Auth.Commands;
using BT.Application.Mappings;
using BT.Application.Utilities;
using BT.Domain.Entities;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;

namespace BT.Infrastructure.Features.Auth.AspNetCoreIdentity.Handlers;


internal sealed class Login(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    IJwtService jwtService,
    IClaimsService claimsService,
    IServiceManager serviceManager,
    ILogger<Login> logger) : IRequestHandler<LoginCommand, AppResponse<LoginResponse>>
{
    public async Task<AppResponse<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var loginRequest = command.LoginRequest;

        try
        {
            var user = await userManager.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == loginRequest.UserName, cancellationToken)
                .ConfigureAwait(false);

            if (user == null)
            {
                ServiceLogDefinitions.LogLoginError(logger, loginRequest.UserName, new AuthenticationException("Invalid user"));
                return AppResponse.Failure<LoginResponse>("Invalid User Name or password.");
            }

            if (!user.IsActive || user.IsDeleted)
            {
                throw new AuthenticationException("User account is disabled.");
            }

            var emailConfirmed = await userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false);
            if (!emailConfirmed)
            {
                ServiceLogDefinitions.LogLoginError(logger, user.UserName ?? string.Empty, new AuthenticationException("Unconfirmed email"));
                return AppResponse.Failure<LoginResponse>("Please confirm your email before logging in.");
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, loginRequest.Password).ConfigureAwait(false);
            if (!passwordValid)
            {
                ServiceLogDefinitions.LogLoginError(logger, user.UserName ?? string.Empty, new AuthenticationException("Invalid password"));
                return AppResponse.Failure<LoginResponse>("Invalid Employee Number or password.");
            }

            var signInResult = await signInManager
                .PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, true)
                .ConfigureAwait(false);

            if (signInResult.IsLockedOut)
            {
                ServiceLogDefinitions.LogLoginError(logger, user.UserName ?? string.Empty, new AuthenticationException("Account locked"));
                return AppResponse.Failure<LoginResponse>("Your account is locked due to multiple failed login attempts. Please reset your password or contact support.");
            }

            if (signInResult.IsNotAllowed)
            {
                ServiceLogDefinitions.LogLoginError(logger, user.UserName ?? string.Empty, new AuthenticationException("Sign in not allowed"));
                return AppResponse.Failure<LoginResponse>("Sign in not allowed. Please contact support.");
            }

            var twoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false);
            if (!twoFactorEnabled)
            {
                ServiceLogDefinitions.LogUsingTempSecret(logger, user.Id);
            }

            var appUserResponse = new AppUserResponse(
                user.Id,
                user.UserName ?? string.Empty,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                $"{user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}".Trim(),
                user.PhoneNumber,
                user.NationalId,
                user.Email ?? string.Empty,
                user.Gender.MapToString(),
                user.ProfilePictureUrl,
                true,
                twoFactorEnabled,
                user.RequirePasswordChange,
                user.CreatedAt,
                user.LastLoginAt,
                [],
                user.TenantId,
                user.EmployeeId,
                user.CustomerId


            );

            if (signInResult.RequiresTwoFactor || twoFactorEnabled)
            {
                var tempClaims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new("temp_auth", "true")
                };

                var tempToken = await jwtService.CreateTempTokenAsync(tempClaims, TimeSpan.FromMinutes(10)).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(tempToken))
                {
                    ServiceLogDefinitions.LogFailedToGenerateAccessToken(logger, user.Id);
                    return AppResponse.Failure<LoginResponse>("Could not generate temporary authentication token");
                }

                var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
                var userInfoWith2FA = appUserResponse with { Roles = [.. roles] };

                await serviceManager.CacheService
                    .SetAsync(CacheKeys.UserInfo(user.Id), userInfoWith2FA, TimeSpan.FromMinutes(10), cancellationToken)
                    .ConfigureAwait(false);

                return AppResponse.Success("Two-factor authentication required", new LoginResponse(
                    user.Id,
                    user.FirstName ?? string.Empty,
                    user.LastName ?? string.Empty,
                    user.Email ?? string.Empty,
                    true,
                    false,
                    false,
                    tempToken,
                    string.Empty,
                    DateTimeOffset.UtcNow.AddMinutes(10),
                    userInfoWith2FA,
                    tempClaims.ToClaimResponses()));
            }

            if (!signInResult.Succeeded)
            {
                ServiceLogDefinitions.LogLoginError(logger, user.UserName ?? string.Empty, new AuthenticationException("Sign in failed"));
                return AppResponse.Failure<LoginResponse>("Invalid login attempt.");
            }

            var sessionId = Guid.CreateVersion7();
            var sessionCreationResult = await serviceManager.SessionService.CreateSessionAsync(
                user.Id,
                sessionId,
                httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown",
                httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "unknown",
                loginRequest.DeviceFingerprint ?? "unknown").ConfigureAwait(false);

            if (!sessionCreationResult.Successful)
            {
                ServiceLogDefinitions.LogFailedToCreateUserSession(logger, user.Id);
                return AppResponse.Failure<LoginResponse>("Could not establish a user session.");
            }

            var userClaims = await claimsService.GetUserClaimsAsync(user).ConfigureAwait(false);
            if (userClaims.Count == 0)
            {
                ServiceLogDefinitions.LogFailedToGetUserClaims(logger, user.Id);
                return AppResponse.Failure<LoginResponse>("Could not retrieve user claims");
            }

            var tokenResponse = await jwtService.CreateTokenAsync(userClaims).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(tokenResponse))
            {
                ServiceLogDefinitions.LogFailedToGenerateAccessToken(logger, user.Id);
                return AppResponse.Failure<LoginResponse>("Could not generate authentication token");
            }

            var refreshToken = jwtService.CreateRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                ServiceLogDefinitions.LogFailedToGenerateRefreshToken(logger, user.Id);
                return AppResponse.Failure<LoginResponse>("Could not generate refresh token");
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(tokenResponse);
            var tokenExpiry = jwt.ValidTo;

            if (!jwtService.IsTokenValid(tokenResponse))
            {
                var tokenException = new SecurityTokenException("Token validation failed");
                ServiceLogDefinitions.LogInvalidTokenWithException(logger, tokenException);
                return AppResponse.Failure<LoginResponse>("Invalid authentication token");
            }

            var rolesResponse = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            var userRoles = new Collection<string>(rolesResponse.ToList());
            var finalAppUserResponse = appUserResponse with { Roles = userRoles };

            await serviceManager.CacheService
                .SetAsync(CacheKeys.UserInfo(user.Id), finalAppUserResponse, TimeSpan.FromMinutes(10), cancellationToken)
                .ConfigureAwait(false);

            await userManager.Users
                .Where(u => u.Id == user.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LastLoginAt, DateTimeOffset.UtcNow), cancellationToken)
                .ConfigureAwait(false);

            return AppResponse.Success("Login successful", new LoginResponse(
                user.Id,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email ?? string.Empty,
                false,
                false,
                true,
                tokenResponse,
                refreshToken,
                tokenExpiry,
                finalAppUserResponse,
                userClaims.ToClaimResponses()));
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogLoginError(logger, loginRequest.UserName, ex);
            throw;
        }
    }
}
