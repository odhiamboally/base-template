using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.Auth.Commands;
using BT.Application.Utilities;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Entities;
using BT.Domain.Events;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.Auth.AspNetCoreIdentity.Handlers;


internal sealed class ResetPassword(
    UserManager<AppUser> userManager,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IHttpContextAccessor httpContextAccessor,
    IPublisher publisher,
    ILogger<ResetPassword> logger) : IRequestHandler<ResetPasswordCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var request = command.Request;

        try
        {
            var user = await userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);
            if (user == null)
            {
                logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
                return AppResponse.Failure<bool>("Invalid reset request");
            }

            // Check OTP verification flag, not Identity token
            var verifiedKey = CacheKeys.PasswordResetVerified(user.Id);
            var isVerified = await cacheService.GetAsync<bool?>(verifiedKey, ct).ConfigureAwait(false);

            if (isVerified != true)
            {
                logger.LogWarning("Password reset attempted without OTP verification for user: {UserId}", user.Id);
                return AppResponse.Failure<bool>("Reset code not verified or expired. Please request a new code.");
            }

            var password = request.Password ?? request.NewPassword ?? string.Empty;

            var isSamePassword = await userManager.CheckPasswordAsync(user, password).ConfigureAwait(false);
            if (isSamePassword)
            {
                return AppResponse.Failure<bool>("New password must be different from your current password");
            }

            // 3. Remove the password directly (no token needed)
            var removeResult = await userManager.RemovePasswordAsync(user).ConfigureAwait(false);
            if (!removeResult.Succeeded)
            {
                logger.LogError("Failed to remove old password for user: {UserId}", user.Id);
                return AppResponse.Failure<bool>("Password reset failed");
            }

            var addResult = await userManager.AddPasswordAsync(user, password).ConfigureAwait(false);
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
                return AppResponse.Failure<bool>("Password reset failed. Please ensure your password meets all requirements.");
            }

            // 4. Update security fields
            user.ResetFailedLoginAttempts();
            user.PasswordLastChanged = DateTimeOffset.UtcNow;
            user.RequirePasswordChange = false;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await userManager.UpdateAsync(user).ConfigureAwait(false);

            // Revoke all sessions
            await unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var refreshTokens = await unitOfWork.TokenRepository.GetActiveTokensByUserIdAsync(user.Id).ConfigureAwait(false);
                if (refreshTokens.Count != 0)
                {
                    var revokedByIp = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    await unitOfWork.TokenRepository.RevokeTokensAsync(refreshTokens, "Password reset", revokedByIp).ConfigureAwait(false);
                }
                return true;

            }).ConfigureAwait(false);

            // Clean up cache - use the new keys
            await cacheService.RemoveAsync(verifiedKey, ct).ConfigureAwait(false);
            await cacheService.RemoveAsync(CacheKeys.PasswordResetOtp(user.Id), ct).ConfigureAwait(false);
            await cacheService.RemoveAsync(CacheKeys.PasswordResetRateLimit(user.Id), ct).ConfigureAwait(false);
            await cacheService.RemoveAsync(CacheKeys.UserInfo(user.Id), ct).ConfigureAwait(false);

            await publisher.Publish(new PasswordResetSuccessEvent(
                user.Id,
                user.Email!,
                $"{user.FirstName} {user.LastName}"), ct).ConfigureAwait(false);

            logger.LogInformation("Password reset successful for user: {UserId}", user.Id);
            return AppResponse.Success("Password reset successfully", true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Password reset failed for email: {Email}.", request.Email);
            return AppResponse.Failure<bool>("Password reset failed. Please try again.");
        }
    }
}
