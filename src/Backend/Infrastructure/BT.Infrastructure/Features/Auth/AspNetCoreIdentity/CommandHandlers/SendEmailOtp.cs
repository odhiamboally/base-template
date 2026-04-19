using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Contracts.Interfaces.Services;
using BT.Application.Features.Auth.Commands;
using BT.Application.Utilities;
using BT.Domain.Entities;
using BT.Domain.Enums;
using BT.Domain.Events;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace BT.Infrastructure.Features.Auth.AspNetCoreIdentity.CommandHandlers;


internal sealed class SendEmailOtp(
    UserManager<AppUser> userManager,
    ICacheService cache,
    IPublisher publisher,
    ILogger<SendEmailOtp> logger) : IRequestHandler<SendEmailOtpCommand, AppResponse<SendEmailOtpResponse>>
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(60);

    public async Task<AppResponse<SendEmailOtpResponse>> Handle(SendEmailOtpCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var user = await userManager.FindByIdAsync(request.UserId).ConfigureAwait(false);

        if (user == null || string.IsNullOrWhiteSpace(user.Email))
            return AppResponse.Failure<SendEmailOtpResponse>("User not found");

        if (!user.IsActive || user.IsDeleted)
            return AppResponse.Failure<SendEmailOtpResponse>("Account is disabled");

        // Only require confirmed email for Login, not for EmailConfirmation or PasswordReset
        var requiresConfirmed = !string.Equals(request.Purpose, OtpPurpose.EmailConfirmation.ToString(), StringComparison.OrdinalIgnoreCase);
        if (requiresConfirmed && !await userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
            return AppResponse.Failure<SendEmailOtpResponse>("Email not confirmed");

        var cooldownKey = CacheKeys.EmailOtpCooldown(user.Id);
        if (await cache.GetAsync<string>(cooldownKey, ct).ConfigureAwait(false) != null)
            return AppResponse.Failure<SendEmailOtpResponse>("Please wait before requesting another code");

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        var otpKey = CacheKeys.EmailOtp(user.Id);
        var hashed = HashCode(user.Id, code, request.Purpose.ToString());

        await cache.SetAsync(otpKey, hashed, OtpLifetime, ct).ConfigureAwait(false);
        await cache.SetAsync(cooldownKey, "1", Cooldown, ct).ConfigureAwait(false);

        var expiresAt = DateTimeOffset.UtcNow.Add(OtpLifetime);

        // Publish to outbox - MassTransit will deliver reliably
        await publisher.Publish(new EmailOtpRequestedEvent(
            user.Id,
            user.Email!,
            user.FirstName,
            code,
            request.Purpose,
            expiresAt), ct).ConfigureAwait(false);

        logger.LogInformation("Email OTP sent to user {UserId} for {Purpose}", user.Id, request.Purpose);
        return AppResponse.Success(
            "Code sent", 
            new SendEmailOtpResponse(
                user.Id, 
                DateTimeOffset.UtcNow.Add(OtpLifetime), 
                (int)Cooldown.TotalSeconds));
    }

    private static string HashCode(string userId, string code, string purpose)
    {
        var input = $"{userId}:{purpose}:{code}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}
