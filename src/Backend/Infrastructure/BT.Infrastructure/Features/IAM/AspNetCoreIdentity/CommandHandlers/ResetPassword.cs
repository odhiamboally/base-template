using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Users.Commands;
using BT.Application.Utilities;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Domain.Features.IAM.Users.Events;
using BT.Infrastructure.Logging;
using BT.Infrastructure.Configuration;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class ResetPassword(
    UserManager<AppUser> userManager,
    IIamUnitOfWork iamUnitOfWork,
    ICacheService cacheService,
    IHttpContextAccessor httpContextAccessor,
    IPublisher publisher,
    IOptions<PasswordRecoverySettings> recoveryOptions,
    ILogger<ResetPassword> logger) : IRequestHandler<ResetPasswordCommand, AppResponse<bool>>
{
    private readonly PasswordRecoverySettings _recoverySettings = recoveryOptions.Value;

    public async Task<AppResponse<bool>> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var request = command.Request;

        try
        {
            var user = await userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);
            if (user == null)
            {
                ServiceLogDefinitions.LogInvalidToken(logger);
                return AppResponses.Failure<bool>("Invalid reset request");
            }

            var password = request.Password ?? request.NewPassword ?? string.Empty;

            var isSamePassword = await userManager.CheckPasswordAsync(user, password).ConfigureAwait(false);
            if (isSamePassword)
            {
                return AppResponses.Failure<bool>("New password must be different from your current password");
            }

            var verifiedKey = CacheKeys.PasswordResetVerified(user.Id);
            IdentityResult passwordResult;

            if (_recoverySettings.Mode == PasswordRecoveryMode.EmailOtp)
            {
                var isVerified = await cacheService.GetAsync<bool?>(verifiedKey, ct).ConfigureAwait(false);
                if (isVerified != true)
                {
                    ServiceLogDefinitions.LogInvalidToken(logger);
                    return AppResponses.Failure<bool>("Reset code not verified or expired. Please request a new code.");
                }

                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
                passwordResult = await userManager.ResetPasswordAsync(user, resetToken, password).ConfigureAwait(false);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    ServiceLogDefinitions.LogInvalidToken(logger);
                    return AppResponses.Failure<bool>("The password reset link is invalid or expired.");
                }

                passwordResult = await userManager.ResetPasswordAsync(user, request.Token, password).ConfigureAwait(false);
            }

            if (!passwordResult.Succeeded)
            {
                var errors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                ServiceLogDefinitions.LogFailedToAddClaim(logger, "password", "reset", user.Id, errors);
                return AppResponses.Failure<bool>("Password reset failed. Please ensure your password meets all requirements.");
            }

            user.CompletePasswordReset(user.Id);

            await userManager.UpdateAsync(user).ConfigureAwait(false);

            await iamUnitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var refreshTokens = await iamUnitOfWork.TokenRepository.GetActiveTokensByUserIdAsync(user.Id).ConfigureAwait(false);
                if (refreshTokens.Any())
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
            return AppResponses.Success("Password reset successfully", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogUnexpectedTokenValidationError(logger, ex);
            return AppResponses.Failure<bool>("Password reset failed. Please try again.");
        }
    }
}
