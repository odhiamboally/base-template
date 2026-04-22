using BT.Application.Contracts.Interfaces.Services;
using BT.Application.Mappings;
using BT.Application.Utilities;
using BT.Domain.Enums;
using BT.Domain.Events;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.IAM.EventHandlers;


internal sealed class EmailOtpRequestedEventHandler(
    IEmailService emailService,
    ILogger<EmailOtpRequestedEventHandler> logger) : INotificationHandler<EmailOtpRequestedEvent>
{
    public async Task Handle(EmailOtpRequestedEvent e, CancellationToken ct)
    {
        // ToDo: Consider using templates
        var (subject, body) = e.Purpose.ToPurposeEnum() switch
        {
            OtpPurpose.Login => ("Your login code",
                $"Hi {e.FirstName},\n\nYour code is: {e.Code}\n\nExpires at {e.ExpiresAt:HH:mm} UTC. Do not share it."),
            OtpPurpose.EmailConfirmation => ("Confirm your email",
                $"Hi {e.FirstName},\n\nYour confirmation code is: {e.Code}\n\nExpires in 5 minutes."),
            OtpPurpose.PasswordReset => ("Password reset code",
                $"Hi {e.FirstName},\n\nYour password reset code is: {e.Code}\n\nExpires in 5 minutes."),
            _ => ("Your verification code", $"Your code is: {e.Code}")
        };

        await emailService.SendEmailAsync(new SendEmailRequest
        {
            To = e.Email,
            Subject = subject,
            Body = body
        }, ct).ConfigureAwait(false);

        LogDefinitions.LogEmailOtpDelivered(logger, e.Purpose, e.UserId);
    }
}
