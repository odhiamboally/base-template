using BT.Application.Contracts.Interfaces.Services;
using BT.Application.Features.IAM.Commands;
using BT.Application.Mappings;
using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.IAM.Entities;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class RefreshToken(
    UserManager<AppUser> userManager,
    IIamUnitOfWork iamUnitOfWork,
    IJwtService jwtService,
    IClaimsService claimsService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RefreshToken> logger) : IRequestHandler<RefreshTokenCommand, AppResponse<RefreshTokenResponse>>
{
    public async Task<AppResponse<RefreshTokenResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        try
        {
            var principal = jwtService.GetPrincipalFromToken(request.AccessToken, false);
            if (principal == null)
            {
                SecurityLogDefinitions.LogSecurityEvent(logger, "InvalidAccessTokenFormat", string.Empty, "Invalid access token format provided for refresh");
                return AppResponse.Failure<RefreshTokenResponse>("Invalid access token format");
            }

            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrWhiteSpace(userId))
            {
                SecurityLogDefinitions.LogSecurityEvent(logger, "InvalidAccessTokenFormat", string.Empty, "No user ID found in access token");
                return AppResponse.Failure<RefreshTokenResponse>("Invalid token: No user ID found");
            }

            var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                SecurityLogDefinitions.LogSecurityEvent(logger, "UserNotFoundForRefresh", userId, "User not found for refresh token");
                return AppResponse.Failure<RefreshTokenResponse>("User not found");
            }

            if (!user.IsActive || user.IsDeleted)
            {
                SecurityLogDefinitions.LogSecurityEvent(logger, "InactiveUserRefreshAttempt", userId, "Refresh token attempt for inactive user");
                return AppResponse.Failure<RefreshTokenResponse>("User account is not active");
            }

            var storedRefreshToken = await iamUnitOfWork.TokenRepository.GetRefreshTokenAsync(request.RefreshToken, userId).ConfigureAwait(false);
            if (storedRefreshToken == null)
            {
                SecurityLogDefinitions.LogSecurityEvent(logger, "RefreshTokenNotFound", userId, "Refresh token not found for user");
                return AppResponse.Failure<RefreshTokenResponse>("Invalid refresh token");
            }

            if (storedRefreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                SecurityLogDefinitions.LogSecurityEvent(logger, "ExpiredRefreshToken", userId, "Expired refresh token used");
                await iamUnitOfWork.TokenRepository.RevokeRefreshTokenAsync(storedRefreshToken, "Token expired").ConfigureAwait(false);
                return AppResponse.Failure<RefreshTokenResponse>("Refresh token has expired");
            }

            if (storedRefreshToken.IsRevoked)
            {
                SecurityLogDefinitions.LogSecurityEvent(logger, "RevokedRefreshToken", userId, "Revoked refresh token used");
                return AppResponse.Failure<RefreshTokenResponse>("Refresh token has been revoked");
            }

            if (storedRefreshToken.IsUsed)
            {
                SecurityLogDefinitions.LogSecurityEvent(logger, "TokenReuseDetected", userId, "Already used refresh token attempted");
                await iamUnitOfWork.TokenRepository.RevokeAllUserTokensAsync(userId, "Token reuse detected").ConfigureAwait(false);
                return AppResponse.Failure<RefreshTokenResponse>("Refresh token has already been used");
            }

            if (storedRefreshToken.AppUserId != userId)
            {
                SecurityLogDefinitions.LogSecurityEvent(logger, "RefreshTokenUserMismatch", userId, $"Token UserId: {storedRefreshToken.AppUserId}, Request UserId: {userId}");
                return AppResponse.Failure<RefreshTokenResponse>("Token mismatch");
            }

            var userClaims = await claimsService.GetUserClaimsAsync(user).ConfigureAwait(false);
            if (userClaims.Count == 0)
            {
                ServiceLogDefinitions.LogFailedToGetUserClaims(logger, userId);
                return AppResponse.Failure<RefreshTokenResponse>("Could not retrieve user claims");
            }

            var newAccessTokenResponse = await jwtService.CreateTokenAsync(userClaims).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(newAccessTokenResponse))
            {
                ServiceLogDefinitions.LogFailedToGenerateAccessToken(logger, userId);
                return AppResponse.Failure<RefreshTokenResponse>("Could not generate new access token");
            }

            var newRefreshTokenResponse = jwtService.CreateRefreshToken();
            if (string.IsNullOrWhiteSpace(newRefreshTokenResponse))
            {
                ServiceLogDefinitions.LogFailedToGenerateRefreshToken(logger, userId);
                return AppResponse.Failure<RefreshTokenResponse>("Could not generate new refresh token");
            }

            var tokenExpiry = jwtService.GetTokenExpiry(newAccessTokenResponse);
            if (tokenExpiry == default)
            {
                ServiceLogDefinitions.LogFailedToGetTokenExpiry(logger, userId);
                return AppResponse.Failure<RefreshTokenResponse>("Could not determine token expiry");
            }

            await iamUnitOfWork.TokenRepository.MarkTokenAsUsedAsync(storedRefreshToken).ConfigureAwait(false);

            var newRefreshTokenEntity = new BT.Domain.IAM.Entities.RefreshToken
            {
                Token = newRefreshTokenResponse,
                AppUserId = userId,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = string.Empty,
                CreatedByIp = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            };

            await iamUnitOfWork.TokenRepository.AddRefreshTokenAsync(newRefreshTokenEntity).ConfigureAwait(false);
            await iamUnitOfWork.TokenRepository.CleanupExpiredTokensAsync(userId).ConfigureAwait(false);

            user.UpdatedAt = DateTimeOffset.UtcNow;
            await userManager.UpdateAsync(user).ConfigureAwait(false);

            var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            var twoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false);

            var userInfo = new AppUserResponse(
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
                user.IsActive,
                twoFactorEnabled,
                user.RequirePasswordChange,
                user.CreatedAt,
                user.LastLoginAt,
                [.. roles],
                user.TenantId,
                user.EmployeeId,
                user.CustomerId);

            ServiceLogDefinitions.LogTokenRefreshed(logger, userId);

            return AppResponse.Success("Token refreshed successfully", new RefreshTokenResponse(
                newAccessTokenResponse,
                newRefreshTokenResponse,
                userId,
                tokenExpiry,
                tokenExpiry,
                userInfo,
                userClaims));
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogTokenRefreshError(logger, ex, request.AccessToken?.Length ?? 0, !string.IsNullOrWhiteSpace(request.RefreshToken));

            return AppResponse.Failure<RefreshTokenResponse>("Unable to refresh token. Please sign in again.");
        }
    }
}
