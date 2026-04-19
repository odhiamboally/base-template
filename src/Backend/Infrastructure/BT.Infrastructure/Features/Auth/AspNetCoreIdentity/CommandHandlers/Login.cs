using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Contracts.Interfaces.Services;
using BT.Application.Extensions;
using BT.Application.Features.Auth.Commands;
using BT.Application.Mappings;
using BT.Application.Utilities;
using BT.Domain.Entities;
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
                logger.LogWarning("Login attempt with invalid user name: {UserName}", loginRequest.UserName);
                return AppResponse.Failure<LoginResponse>("Invalid User Name or password.");
            }

            if (!user.IsActive || user.IsDeleted)
            {
                throw new AuthenticationException("User account is disabled.");
            }

            var emailConfirmed = await userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false);
            if (!emailConfirmed)
            {
                logger.LogWarning("Login attempt with unconfirmed email for user: {UserId}", user.Id);
                return AppResponse.Failure<LoginResponse>("Please confirm your email before logging in.");
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, loginRequest.Password).ConfigureAwait(false);
            if (!passwordValid)
            {
                logger.LogWarning("Invalid password attempt for user: {UserId}", user.Id);
                return AppResponse.Failure<LoginResponse>("Invalid Employee Number or password.");
            }

            var signInResult = await signInManager
                .PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, true)
                .ConfigureAwait(false);

            if (signInResult.IsLockedOut)
            {
                logger.LogWarning("Account locked for user: {UserId}", user.Id);
                return AppResponse.Failure<LoginResponse>("Your account is locked due to multiple failed login attempts. Please reset your password or contact support.");
            }

            if (signInResult.IsNotAllowed)
            {
                logger.LogWarning("Sign in not allowed for user: {UserId}", user.Id);
                return AppResponse.Failure<LoginResponse>("Sign in not allowed. Please contact support.");
            }

            var twoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false);
            if (!twoFactorEnabled)
            {
                logger.LogInformation("User {UserId} does not have 2FA enabled", user.Id);
            }

            var appUserResponse = new AppUserResponse(
                user.Id,
                user.UserName ?? string.Empty,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                $"{user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}".Trim(),
                user.PhoneNumber,
                user.IdNumber,
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
                user.MemberId


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
                    logger.LogError("Failed to generate temporary token for user: {UserId}", user.Id);
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
                logger.LogWarning("Sign in failed for user: {UserId}", user.Id);
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
                logger.LogError("Failed to create user session for user {UserId}", user.Id);
                return AppResponse.Failure<LoginResponse>("Could not establish a user session.");
            }

            var userClaims = await claimsService.GetUserClaimsAsync(user).ConfigureAwait(false);
            if (!userClaims.Any())
            {
                logger.LogError("Failed to get user claims for user: {UserId}", user.Id);
                return AppResponse.Failure<LoginResponse>("Could not retrieve user claims");
            }

            var tokenResponse = await jwtService.CreateTokenAsync(userClaims).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(tokenResponse))
            {
                logger.LogError("Failed to generate token for user: {UserId}", user.Id);
                return AppResponse.Failure<LoginResponse>("Could not generate authentication token");
            }

            var refreshToken = jwtService.CreateRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                logger.LogError("Failed to generate refresh token for user: {UserId}", user.Id);
                return AppResponse.Failure<LoginResponse>("Could not generate refresh token");
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(tokenResponse);
            var tokenExpiry = jwt.ValidTo;

            if (!jwtService.IsTokenValid(tokenResponse))
            {
                logger.LogError("Token validation failed for user: {UserId}", user.Id);
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
            logger.LogError(ex, "Error during login for User: {UserName}", loginRequest.UserName);
            throw;
        }
    }
}
