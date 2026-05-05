using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.SharedKernel.Extensions;
using BT.Application.Features.IAM.Users.Commands;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class VerifyEmailOtp(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IClaimsService claimsService,
    IJwtService jwtService,
    ICacheService cache,
    ILogger<VerifyEmailOtp> logger) : IRequestHandler<VerifyEmailOtpCommand, AppResponse<VerifyEmailOtpResponse>>
{
    public async Task<AppResponse<VerifyEmailOtpResponse>> Handle(VerifyEmailOtpCommand command, CancellationToken ct)
    {
        var req = command.Request;
        var user = await userManager.FindByIdAsync(req.UserId).ConfigureAwait(false);
        if (user == null) return AppResponse.Failure<VerifyEmailOtpResponse>("User not found");

        var attemptKey = CacheKeys.EmailOtpAttempts(user.Id);
        var attempts = await cache.GetAsync<int?>(attemptKey, ct).ConfigureAwait(false) ?? 0;
        if (attempts >= 3)
            return AppResponse.Failure<VerifyEmailOtpResponse>("Too many attempts. Request a new code.");

        var otpKey = CacheKeys.EmailOtp(user.Id);
        var storedHash = await cache.GetAsync<string>(otpKey, ct).ConfigureAwait(false);
        if (storedHash == null)
            return AppResponse.Failure<VerifyEmailOtpResponse>("Code expired. Request a new code.");

        var providedHash = HashCode(user.Id, req.Code, req.Purpose);
        var isValid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(storedHash),
            Encoding.UTF8.GetBytes(providedHash));

        if (!isValid)
        {
            await cache.SetAsync(attemptKey, attempts + 1, TimeSpan.FromMinutes(10), ct).ConfigureAwait(false);
            ServiceLogDefinitions.LogInvalidEmailOtp(logger, user.Id);
            return AppResponse.Failure<VerifyEmailOtpResponse>("Invalid code");
        }

        await cache.RemoveAsync(otpKey, ct).ConfigureAwait(false);
        await cache.RemoveAsync(attemptKey, ct).ConfigureAwait(false);

        if (string.Equals(req.Purpose, "EmailConfirmation", StringComparison.OrdinalIgnoreCase))
        {
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user).ConfigureAwait(false);
                ServiceLogDefinitions.LogEmailConfirmedViaOtp(logger, user.Id);
            }
            return AppResponse.Success("Email confirmed",
                new VerifyEmailOtpResponse(string.Empty, string.Empty, user.Id, true, DateTimeOffset.UtcNow, null!, []));
        }

        if (string.Equals(req.Purpose, "PasswordReset", StringComparison.OrdinalIgnoreCase))
        {
            await cache.SetAsync(CacheKeys.PasswordResetVerified(user.Id), true, TimeSpan.FromMinutes(5), ct).ConfigureAwait(false);
            return AppResponse.Success("Code verified",
                new VerifyEmailOtpResponse(string.Empty, string.Empty, user.Id, true, DateTimeOffset.UtcNow, null!, []));
        }

        if (!string.Equals(req.Purpose, "Login", StringComparison.OrdinalIgnoreCase))
        {
            return AppResponse.Success("Code verified",
                new VerifyEmailOtpResponse(string.Empty, string.Empty, user.Id, true, DateTimeOffset.UtcNow, null!, []));
        }

        var claims = await claimsService.GetUserClaimsAsync(user).ConfigureAwait(false);
        var token = await jwtService.CreateTokenAsync(claims).ConfigureAwait(false);
        var refreshToken = jwtService.CreateRefreshToken();
        var expiry = jwtService.GetTokenExpiry(token);

        user.RecordSuccessfulLogin();
        await userManager.UpdateAsync(user).ConfigureAwait(false);

        if (req.RememberDevice) await signInManager.RememberTwoFactorClientAsync(user).ConfigureAwait(false);
        await signInManager.SignInAsync(user, req.RememberMe).ConfigureAwait(false);

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        var twoFactor = await userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false);

        var appUser = new AppUserResponse(
            user.Id, user.UserName ?? "", user.FirstName, user.LastName,
            $"{user.FirstName} {user.LastName}".Trim(), user.PhoneNumber, user.NationalId,
            user.Email ?? "", user.Gender.ToDisplayString(), user.ProfilePictureUrl,
            user.IsActive, twoFactor, user.RequirePasswordChange, user.CreatedAt,
            user.LastLoginAt, [.. roles], user.TenantId, user.EmployeeId, user.CustomerId);

        var claimsResp = claims.Select(c => new UserClaimsResponse
        {
            Type = c.Type,
            Value = c.Value,
            ValueType = c.ValueType,
            Issuer = c.Issuer,
            OriginalIssuer = c.OriginalIssuer,
            Properties = c.Properties.ToDictionary(k => k.Key, v => v.Value)

        }).ToList();

        return AppResponse.Success("Email OTP verified", new VerifyEmailOtpResponse(
            token,
            refreshToken ?? "",
            user.Id,
            true,
            expiry,
            appUser,
            claimsResp));
    }

    private static string HashCode(string userId, string code, string purpose)
    {
        var input = $"{userId}:{purpose}:{code}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}
