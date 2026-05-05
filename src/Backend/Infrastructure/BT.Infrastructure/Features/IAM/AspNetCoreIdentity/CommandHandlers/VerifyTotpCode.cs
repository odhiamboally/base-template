using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.SharedKernel.Extensions;
using BT.Application.Features.IAM.Users.Commands;
using BT.Application.Features.Banking.Customers.Mappings;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Features.IAM.Users.Mappings;
using BT.Application.Features.Shared.EmailTemplates.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OtpNet;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class VerifyTotpCode(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IClaimsService claimsService,
    IJwtService jwtService,
    ICacheService cache,
    IEncryptionService encryptionService,
    IIamUnitOfWork iamUnitOfWork,
    ILogger<VerifyTotpCode> logger) : IRequestHandler<VerifyOtpCommand, AppResponse<VerifyOtpResponse>>
{
    public async Task<AppResponse<VerifyOtpResponse>> Handle(VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        try
        {
            var user = await userManager.FindByIdAsync(request.UserId).ConfigureAwait(false);
            if (user == null)
            {
                ServiceLogDefinitions.Log2FAVerificationAttemptNonExistentUser(logger, request.UserId);
                return AppResponse.Failure<VerifyOtpResponse>("User not found");
            }

            bool isValidCode;

            var tempSecret = await iamUnitOfWork.TempTotpSecretRepository
                .GetValidTempSecretByUserIdAsync(user.Id)
                .ConfigureAwait(false);

            if (tempSecret != null)
            {
                ServiceLogDefinitions.LogUsingTempSecret(logger, user.Id);
                var decryptedSecret = encryptionService.Decrypt(tempSecret.EncryptedSecret);
                isValidCode = VerifyTotp(decryptedSecret, request.Code);

                if (isValidCode)
                {
                    var newSecret = AppUserTotpSecret.Create(
                        user.Id,
                        tempSecret.EncryptedSecret,
                        user.Id);

                    await iamUnitOfWork.AppUserTotpSecretRepository.CreateAsync(newSecret, cancellationToken).ConfigureAwait(false);
                    await iamUnitOfWork.TempTotpSecretRepository.DeleteAsync(tempSecret.Id, cancellationToken).ConfigureAwait(false);
                    await iamUnitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

                    await userManager.SetTwoFactorEnabledAsync(user, true).ConfigureAwait(false);
                    ServiceLogDefinitions.LogOtpEnabled(logger, user.Id);
                }
            }
            else
            {
                isValidCode = await VerifyUserTotpAsync(user.Id, request.Code, cancellationToken).ConfigureAwait(false);
            }

            if (!isValidCode)
            {
                ServiceLogDefinitions.LogInvalidOtpCode(logger, request.UserId);
                return AppResponse.Failure<VerifyOtpResponse>("Invalid verification code. Please try again.");
            }

            var userClaims = await claimsService.GetUserClaimsAsync(user).ConfigureAwait(false);
            if (!userClaims.Any())
            {
                return AppResponse.Failure<VerifyOtpResponse>("Could not retrieve user claims");
            }

            var tokenResponse = await jwtService.CreateTokenAsync(userClaims).ConfigureAwait(false);
            var tokenExpiry = jwtService.GetTokenExpiry(tokenResponse);
            var refreshToken = jwtService.CreateRefreshToken();

            user.RecordSuccessfulLogin();
            await userManager.UpdateAsync(user).ConfigureAwait(false);

            if (request.RememberDevice)
                await signInManager.RememberTwoFactorClientAsync(user).ConfigureAwait(false);

            await signInManager.SignInAsync(user, request.RememberMe).ConfigureAwait(false);

            var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            var twoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false);

            var appUserResponse = new AppUserResponse(
                user.Id,
                user.UserName ?? string.Empty,
                user.FirstName,
                user.LastName,
                $"{user.FirstName} {user.LastName}".Trim(),
                user.PhoneNumber,
                user.NationalId,
                user.Email ?? string.Empty,
                user.Gender.ToDisplayString(),
                user.ProfilePictureUrl,
                user.IsActive,
                twoFactorEnabled,
                user.RequirePasswordChange,
                user.CreatedAt,
                user.LastLoginAt,
                [.. roles],
                user.TenantId,
                user.EmployeeId,
                user.CustomerId
            );

            var claimsResponse = userClaims.Select(c => new UserClaimsResponse
            {
                Type = c.Type,
                Value = c.Value,
                ValueType = c.ValueType,
                Issuer = c.Issuer,
                OriginalIssuer = c.OriginalIssuer,
                Properties = c.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            }).ToList();

            return AppResponse.Success("OTP verification successful",
                new VerifyOtpResponse(
                    tokenResponse,
                    refreshToken ?? string.Empty,
                    user.Id,
                    true,
                    tokenExpiry,
                    appUserResponse,
                    claimsResponse));
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogErrorVerifyingPassword(logger, ex);
            throw;
        }
    }

    private async Task<bool> VerifyUserTotpAsync(string userId, string code, CancellationToken ct)
    {
        var attemptKey = CacheKeys.TotpAttempts(userId);
        var attempts = await cache.GetAsync<int?>(attemptKey, ct).ConfigureAwait(false) ?? 0;

        if (attempts >= 3)
            return false;

        var secretEntity = await iamUnitOfWork.AppUserTotpSecretRepository.GetActiveSecretByUserIdAsync(userId).ConfigureAwait(false);
        if (secretEntity == null) return false;

        var decryptedSecret = encryptionService.Decrypt(secretEntity.EncryptedSecret);
        var isValid = VerifyTotp(decryptedSecret, code);

        if (!isValid)
        {
            attempts++;
            await cache.SetAsync(attemptKey, attempts, TimeSpan.FromMinutes(10), ct).ConfigureAwait(false);
            return false;
        }

        await cache.RemoveAsync(attemptKey, ct).ConfigureAwait(false);
        secretEntity.MarkAsUsed();

        await iamUnitOfWork.AppUserTotpSecretRepository.UpdateAsync(secretEntity).ConfigureAwait(false);
        await iamUnitOfWork.CompleteAsync(ct).ConfigureAwait(false);

        return true;
    }

    private bool VerifyTotp(string secret, string code, int windowSize = 2)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code) || code.Length != 6)
        {
            return false;
        }

        try
        {
            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes, step: 30, mode: OtpHashMode.Sha1, totpSize: 6);
            var window = new VerificationWindow(windowSize, windowSize);
            return totp.VerifyTotp(code.Trim(), out _, window);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogTotpPlainTextCodeVerificationError(logger, ex);
            return false;
        }
    }
}
