using BT.SharedKernel.Features.Shared.Notifications.Dtos;
using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.SharedKernel.Extensions;
using BT.Application.Features.Banking.Customers.Mappings;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Features.IAM.Users.Mappings;
using BT.Application.Features.Shared.EmailTemplates.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Users.Enums;
using BT.Domain.Features.IAM.Users.Events;
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
        var (subject, body) = e.Purpose.ToEnum<OtpPurpose>() switch
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
