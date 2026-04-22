using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Contracts.Interfaces.Services;
using BT.Application.Extensions;
using BT.Application.Features.Auth.Commands;
using BT.Application.Mappings;
using BT.Application.Utilities;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Entities;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OtpNet;

namespace BT.Infrastructure.Features.Auth.AspNetCoreIdentity.Handlers;


internal sealed class VerifyTotpCode(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IClaimsService claimsService,
    IJwtService jwtService,
    ICacheService cache,
    IEncryptionService encryptionService,
    IIamUnitOfWork unitOfWork,
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
                logger.LogWarning("2FA verification attempt for non-existent user: {UserId}", request.UserId);
                return AppResponse.Failure<VerifyOtpResponse>("User not found");
            }

            bool isValidCode;

            // Check for temp secret (setup flow) first
            var tempSecret = await unitOfWork.TempTotpSecretRepository
                .GetValidTempSecretByUserIdAsync(user.Id)
                .ConfigureAwait(false);

            if (tempSecret != null)
            {
                logger.LogInformation("Using temp secret for OTP setup for user: {UserId}", user.Id);
                var decryptedSecret = encryptionService.Decrypt(tempSecret.EncryptedSecret);
                isValidCode = VerifyTotp(decryptedSecret, request.Code);

                if (isValidCode)
                {
                    var newSecret = new AppUserTotpSecret
                    {
                        AppUserId = user.Id,
                        EncryptedSecret = tempSecret.EncryptedSecret,
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = user.Id,
                    };

                    await unitOfWork.AppUserTotpSecretRepository.CreateAsync(newSecret, cancellationToken).ConfigureAwait(false);
                    await unitOfWork.TempTotpSecretRepository.DeleteAsync(tempSecret.Id, cancellationToken).ConfigureAwait(false);
                    await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

                    await userManager.SetTwoFactorEnabledAsync(user, true).ConfigureAwait(false);
                    logger.LogInformation("OTP enabled for user: {UserId}", user.Id);
                }
            }
            else
            {
                // Normal login flow - verify against stored secret with lockout
                isValidCode = await VerifyUserTotpAsync(user.Id, request.Code, cancellationToken).ConfigureAwait(false);
            }

            if (!isValidCode)
            {
                logger.LogWarning("Invalid OTP code for user: {UserId}", request.UserId);
                return AppResponse.Failure<VerifyOtpResponse>("Invalid verification code. Please try again.");
            }

            // Success - issue tokens
            var userClaims = await claimsService.GetUserClaimsAsync(user).ConfigureAwait(false);
            if (userClaims.Count == 0)
            {
                return AppResponse.Failure<VerifyOtpResponse>("Could not retrieve user claims");
            }

            var tokenResponse = await jwtService.CreateTokenAsync(userClaims).ConfigureAwait(false);
            var tokenExpiry = jwtService.GetTokenExpiry(tokenResponse);
            var refreshToken = jwtService.CreateRefreshToken();

            user.LastLoginAt = DateTimeOffset.UtcNow;
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
            logger.LogError(ex, "Error during 2FA verification for user: {UserId}", request.UserId);
            throw;
        }
    }

    private async Task<bool> VerifyUserTotpAsync(string userId, string code, CancellationToken ct)
    {
        var attemptKey = CacheKeys.TotpAttempts(userId);
        var attempts = await cache.GetAsync<int?>(attemptKey, ct).ConfigureAwait(false) ?? 0;

        if (attempts >= 3)
            return false;

        var secretEntity = await unitOfWork.AppUserTotpSecretRepository.GetActiveSecretByUserIdAsync(userId).ConfigureAwait(false);
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
        secretEntity.LastUsedAt = DateTimeOffset.UtcNow;

        await unitOfWork.AppUserTotpSecretRepository.UpdateAsync(secretEntity).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(ct).ConfigureAwait(false);

        return true;
    }

    private static bool VerifyTotp(string secret, string code, int windowSize = 2)
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
        catch
        {
            return false;
        }
    }

    
}