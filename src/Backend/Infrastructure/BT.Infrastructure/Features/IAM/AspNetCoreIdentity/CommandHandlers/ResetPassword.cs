using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Commands;
using BT.Application.Utilities;
using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.IAM.Entities;
using BT.Domain.IAM.Events;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class ResetPassword(
    UserManager<AppUser> userManager,
    IIamUnitOfWork iamUnitOfWork,
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
                ServiceLogDefinitions.LogInvalidToken(logger);
                return AppResponse.Failure<bool>("Invalid reset request");
            }

            var verifiedKey = CacheKeys.PasswordResetVerified(user.Id);
            var isVerified = await cacheService.GetAsync<bool?>(verifiedKey, ct).ConfigureAwait(false);

            if (isVerified != true)
            {
                ServiceLogDefinitions.LogInvalidToken(logger);
                return AppResponse.Failure<bool>("Reset code not verified or expired. Please request a new code.");
            }

            var password = request.Password ?? request.NewPassword ?? string.Empty;

            var isSamePassword = await userManager.CheckPasswordAsync(user, password).ConfigureAwait(false);
            if (isSamePassword)
            {
                return AppResponse.Failure<bool>("New password must be different from your current password");
            }

            var removeResult = await userManager.RemovePasswordAsync(user).ConfigureAwait(false);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                ServiceLogDefinitions.LogErrorUpdatingClaim(logger, user.Id,
                    new InvalidOperationException($"Failed to remove old password: {errors}"));

                return AppResponse.Failure<bool>("Password reset failed");
            }

            var addResult = await userManager.AddPasswordAsync(user, password).ConfigureAwait(false);
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                ServiceLogDefinitions.LogFailedToAddClaim(logger, "password", "reset", user.Id, errors);
                return AppResponse.Failure<bool>("Password reset failed. Please ensure your password meets all requirements.");
            }

            user.ResetFailedLoginAttempts();
            user.PasswordLastChanged = DateTimeOffset.UtcNow;
            user.RequirePasswordChange = false;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await userManager.UpdateAsync(user).ConfigureAwait(false);

            await iamUnitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var refreshTokens = await iamUnitOfWork.TokenRepository.GetActiveTokensByUserIdAsync(user.Id).ConfigureAwait(false);
                if (refreshTokens.Count != 0)
                {
                    var revokedByIp = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    await iamUnitOfWork.TokenRepository.RevokeTokensAsync(refreshTokens, "Password reset", revokedByIp).ConfigureAwait(false);
                }
                return true;

            }).ConfigureAwait(false);

            await cacheService.RemoveAsync(verifiedKey, ct).ConfigureAwait(false);
            await cacheService.RemoveAsync(CacheKeys.PasswordResetOtp(user.Id), ct).ConfigureAwait(false);
            await cacheService.RemoveAsync(CacheKeys.PasswordResetRateLimit(user.Id), ct).ConfigureAwait(false);
            await cacheService.RemoveAsync(CacheKeys.UserInfo(user.Id), ct).ConfigureAwait(false);

            await publisher.Publish(new PasswordResetSuccessEvent(
                user.Id,
                user.Email!,
                $"{user.FirstName} {user.LastName}"), ct).ConfigureAwait(false);

            ServiceLogDefinitions.LogEmailOtpSent(logger, user.Id, "PasswordReset");
            return AppResponse.Success("Password reset successfully", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogUnexpectedTokenValidationError(logger, ex);
            return AppResponse.Failure<bool>("Password reset failed. Please try again.");
        }
    }
}
