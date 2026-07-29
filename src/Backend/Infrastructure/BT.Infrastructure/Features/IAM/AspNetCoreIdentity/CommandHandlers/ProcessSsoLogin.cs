using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.IAM.Users.Commands;
using BT.Application.Features.IAM.Users.Mappings;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Domain.Shared.Contracts.Common;
using BT.Infrastructure.Configuration;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.SharedKernel.Features.Shared.Common.Enums;
using BT.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class ProcessSsoLogin(
    UserManager<AppUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IJwtService jwtService,
    IClaimsService claimsService,
    IServiceManager serviceManager,
    IIamUnitOfWork iamUnitOfWork,
    ICurrentTenantProvider currentTenantProvider,
    IOptions<JwtSettings> jwtSettings,
    IOptions<EntraIdSettings> entraIdSettings,
    ILogger<ProcessSsoLogin> logger) : IRequestHandler<ProcessSsoLoginCommand, AppResponse<string>>
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly EntraIdSettings _entraIdSettings = entraIdSettings.Value;

    public async Task<AppResponse<string>> Handle(ProcessSsoLoginCommand command, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantProvider.TenantId;

        var user = await userManager.FindByLoginAsync(command.Provider, command.ProviderKey).ConfigureAwait(false);

        if (user == null)
        {
            user = await userManager.FindByEmailAsync(command.Email).ConfigureAwait(false);

            if (user != null)
            {
                var linkResult = await userManager.AddLoginAsync(user, new UserLoginInfo(command.Provider, command.ProviderKey, command.Provider)).ConfigureAwait(false);
                if (!linkResult.Succeeded)
                {
                    logger.LogError("Failed to link external login for user {Email}", command.Email);
                    return AppResponses.Failure<string>("Failed to link external account.");
                }
            }
            else if (_entraIdSettings.AutoLinkByVerifiedEmail)
            {
                user = AppUser.CreateExternalUser(
                    tenantId,
                    command.Email,
                    command.Email,
                    command.FirstName,
                    command.LastName,
                    "System"
                );

                var createResult = await userManager.CreateAsync(user).ConfigureAwait(false);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to create SSO user for {Email}: {Errors}", command.Email, errors);
                    return AppResponses.Failure<string>("Failed to provision user.");
                }

                await userManager.AddToRoleAsync(user, "User").ConfigureAwait(false);

                await userManager.AddLoginAsync(user, new UserLoginInfo(command.Provider, command.ProviderKey, command.Provider)).ConfigureAwait(false);
            }
            else
            {
                return AppResponses.Failure<string>("User not found and auto-provisioning is disabled.");
            }
        }

        if (!user.IsActive || user.IsDeleted)
        {
            return AppResponses.Failure<string>("This account is inactive. Please contact support.");
        }

        var sessionId = Guid.CreateVersion7();
        var sessionCreationResult = await serviceManager.SessionService.CreateSessionAsync(
            user.Id,
            sessionId,
            httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown",
            httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "unknown",
            "SSO",
            cancellationToken).ConfigureAwait(false);

        if (!sessionCreationResult.IsSuccess)
        {
            return AppResponses.Failure<string>("Could not establish a user session.");
        }

        var activeSessionId = sessionCreationResult.Data;
        var userClaims = await claimsService.GetUserClaimsAsync(user, activeSessionId).ConfigureAwait(false);
        var tokenResponse = await jwtService.CreateTokenAsync(userClaims).ConfigureAwait(false);
        var refreshToken = jwtService.CreateRefreshToken();

        var refreshTokenEntity = BT.Domain.Features.IAM.Users.Entities.RefreshToken.Create(
            user.Id,
            refreshToken,
            DateTimeOffset.UtcNow.AddHours(_jwtSettings.RefreshTokenExpiryHours),
            user.Id,
            httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString());

        user.RecordSuccessfulLogin();
        await userManager.UpdateAsync(user).ConfigureAwait(false);

        await iamUnitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
        {
            await iamUnitOfWork.TokenRepository.AddRefreshTokenAsync(refreshTokenEntity).ConfigureAwait(false);
            await iamUnitOfWork.TokenRepository.CleanupExpiredTokensAsync(user.Id).ConfigureAwait(false);
            await iamUnitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }).ConfigureAwait(false);

        var rolesResponse = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        
        var appUserResponse = new AppUserResponse(
                user.Id,
                user.UserName ?? string.Empty,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                $"{user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}".Trim(),
                user.PhoneNumber,
                user.NationalId,
                user.Email ?? string.Empty,
                user.Gender.ToDisplayString(),
                ProfilePictureUrlMapping.ToCurrentUserRoute(user.ProfilePictureUrl),
                true,
                false,
                user.RequirePasswordChange,
                user.CreatedAt,
                user.LastLoginAt,
                [..rolesResponse],
                user.TenantId,
                user.EmployeeId,
                user.CustomerId);

        var loginResponse = new LoginResponse(
                user.Id,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email ?? string.Empty,
                false,
                false,
                true,
                tokenResponse,
                refreshToken,
                activeSessionId.ToString(),
                DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                appUserResponse,
                userClaims.ToClaimResponses(),
                false);

        var exchangeCode = Guid.NewGuid().ToString("N");
        
        await serviceManager.CacheService.SetAsync(
            $"SSO_Exchange_{exchangeCode}", 
            loginResponse, 
            TimeSpan.FromMinutes(2), 
            cancellationToken).ConfigureAwait(false);

        return AppResponses.Success("SSO Login successful", exchangeCode);
    }
}
