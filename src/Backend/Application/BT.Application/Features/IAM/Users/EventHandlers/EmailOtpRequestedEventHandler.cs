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

namespace BT.Application.Features.IAM.Users.EventHandlers;


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
                $"<p>Hi {e.FirstName},</p><p>Your code is: <strong>{e.Code}</strong></p><p>Expires at {e.ExpiresAt:HH:mm} UTC. Do not share it.</p>"),
            OtpPurpose.EmailConfirmation => ("Confirm your email",
                $"<p>Hi {e.FirstName},</p><p>Your confirmation code is: <strong>{e.Code}</strong></p><p>Expires in 5 minutes.</p>"),
            OtpPurpose.PasswordReset => ("Password reset code",
                $"<p>Hi {e.FirstName},</p><p>Your password reset code is: <strong>{e.Code}</strong></p><p>Expires in 5 minutes.</p>"),
            _ => ("Your verification code", $"<p>Your code is: <strong>{e.Code}</strong></p>")
        };

        var result = await emailService.SendEmailAsync(new SendEmailRequest
        {
            To = e.Email,
            Subject = subject,
            Body = body
        }, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            LogDefinitions.LogEmailOtpDelivered(logger, e.Purpose, e.UserId);
        }
        else
        {
            LogDefinitions.LogEmailOtpFailedToDeliver(logger, e.Purpose, e.UserId, result.Message ?? "Unknown error");
        }
    }
}
