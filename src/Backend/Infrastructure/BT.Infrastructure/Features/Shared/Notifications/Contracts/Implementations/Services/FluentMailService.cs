using BT.SharedKernel.Features.Shared.Notifications.Dtos;
using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Common;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Features.Shared.Notifications.Contracts.Implementations.Services;

internal sealed class FluentMailService(IFluentEmail _fluentEmail, ILogger<FluentMailService> _logger) : IEmailService
{
    public async Task<AppResponse<SendEmailResponse>> SendEmailAsync(SendEmailRequest sendEmailRequest, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(sendEmailRequest);

            if (string.IsNullOrWhiteSpace(sendEmailRequest.To))
                throw new ArgumentException("Recipient email is required");

            var email = _fluentEmail
                .To(sendEmailRequest.To)
                .Subject(sendEmailRequest.Subject ?? string.Empty)
                .Body(sendEmailRequest.Body ?? string.Empty, true); // true = HTML

            var result = await email.SendAsync(cancellationToken).ConfigureAwait(false);

            if (result.Successful)
            {
                ServiceLogDefinitions.LogEmailSent(_logger, sendEmailRequest.To);

                return AppResponse.Success("Email sent successfully",
                    new SendEmailResponse(
                        Guid.NewGuid().ToString(),
                        DateTimeOffset.UtcNow,
                        sendEmailRequest.To,
                        sendEmailRequest.Subject ?? string.Empty)
                    );
            }
            else
            {
                var errorMessage = string.Join(", ", result.ErrorMessages);
                ServiceLogDefinitions.LogFailedToSendEmail(_logger, sendEmailRequest.To, errorMessage);

                return AppResponse.Failure<SendEmailResponse>($"Failed to send email: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogErrorSendingEmail(_logger, sendEmailRequest?.To ?? string.Empty, ex);
            throw;
        }
    }



}
