using BT.Application.Contracts.Interfaces.Services;
using BT.Application.Features.Auth.Commands;
using BT.Application.Mappings;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Entities;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BT.Infrastructure.Features.Auth.AspNetCoreIdentity.Handlers;


internal sealed class RefreshToken(
    UserManager<AppUser> userManager,
    IUnitOfWork unitOfWork,
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
                logger.LogWarning("Invalid access token format provided for refresh");
                return AppResponse.Failure<RefreshTokenResponse>("Invalid access token format");
            }

            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.LogWarning("No user ID found in access token");
                return AppResponse.Failure<RefreshTokenResponse>("Invalid token: No user ID found");
            }

            var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                logger.LogWarning("User not found for refresh token: {UserId}", userId);
                return AppResponse.Failure<RefreshTokenResponse>("User not found");
            }

            if (!user.IsActive || user.IsDeleted)
            {
                logger.LogWarning("Refresh token attempt for inactive user: {UserId}", userId);
                return AppResponse.Failure<RefreshTokenResponse>("User account is not active");
            }

            var storedRefreshToken = await unitOfWork.TokenRepository.GetRefreshTokenAsync(request.RefreshToken, userId).ConfigureAwait(false);
            if (storedRefreshToken == null)
            {
                logger.LogWarning("Refresh token not found for user: {UserId}", userId);
                return AppResponse.Failure<RefreshTokenResponse>("Invalid refresh token");
            }

            if (storedRefreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                logger.LogWarning("Expired refresh token used for user: {UserId}", userId);
                await unitOfWork.TokenRepository.RevokeRefreshTokenAsync(storedRefreshToken, "Token expired").ConfigureAwait(false);
                return AppResponse.Failure<RefreshTokenResponse>("Refresh token has expired");
            }

            if (storedRefreshToken.IsRevoked)
            {
                logger.LogWarning("Revoked refresh token used for user: {UserId}", userId);
                return AppResponse.Failure<RefreshTokenResponse>("Refresh token has been revoked");
            }

            if (storedRefreshToken.IsUsed)
            {
                logger.LogWarning("Already used refresh token attempted for user: {UserId}", userId);
                await unitOfWork.TokenRepository.RevokeAllUserTokensAsync(userId, "Token reuse detected").ConfigureAwait(false);
                return AppResponse.Failure<RefreshTokenResponse>("Refresh token has already been used");
            }

            if (storedRefreshToken.AppUserId != userId)
            {
                logger.LogWarning("Refresh token user mismatch. Token UserId: {TokenUserId}, Request UserId: {RequestUserId}",
                    storedRefreshToken.AppUserId, userId);
                return AppResponse.Failure<RefreshTokenResponse>("Token mismatch");
            }

            var userClaims = await claimsService.GetUserClaimsAsync(user).ConfigureAwait(false);
            if (!userClaims.Any())
            {
                logger.LogError("Failed to get user claims during token refresh for user: {UserId}", userId);
                return AppResponse.Failure<RefreshTokenResponse>("Could not retrieve user claims");
            }

            var newAccessTokenResponse = await jwtService.CreateTokenAsync(userClaims).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(newAccessTokenResponse))
            {
                logger.LogError("Failed to generate new access token for user: {UserId}", userId);
                return AppResponse.Failure<RefreshTokenResponse>("Could not generate new access token");
            }

            var newRefreshTokenResponse = jwtService.CreateRefreshToken();
            if (string.IsNullOrWhiteSpace(newRefreshTokenResponse))
            {
                logger.LogError("Failed to generate new refresh token for user: {UserId}", userId);
                return AppResponse.Failure<RefreshTokenResponse>("Could not generate new refresh token");
            }

            var tokenExpiry = jwtService.GetTokenExpiry(newAccessTokenResponse);
            if (tokenExpiry == default)
            {
                logger.LogError("Failed to get token expiry for user: {UserId}", userId);
                return AppResponse.Failure<RefreshTokenResponse>("Could not determine token expiry");
            }

            await unitOfWork.TokenRepository.MarkTokenAsUsedAsync(storedRefreshToken).ConfigureAwait(false);

            var newRefreshTokenEntity = new Domain.Entities.RefreshToken
            {
                Token = newRefreshTokenResponse,
                AppUserId = userId,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = string.Empty,
                CreatedByIp = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            };

            await unitOfWork.TokenRepository.AddRefreshTokenAsync(newRefreshTokenEntity).ConfigureAwait(false);
            await unitOfWork.TokenRepository.CleanupExpiredTokensAsync(userId).ConfigureAwait(false);

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
                user.IdNumber,
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
                user.MemberId


            );

            logger.LogInformation("Token refreshed successfully for user: {UserId}", userId);

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
            logger.LogError(ex,
                "Error refreshing token. AccessTokenLength: {AccessTokenLength}, HasRefreshToken: {HasRefreshToken}",
                request.AccessToken?.Length ?? 0,
                !string.IsNullOrWhiteSpace(request.RefreshToken));

            return AppResponse.Failure<RefreshTokenResponse>("Unable to refresh token. Please sign in again.");
        }
    }
}
