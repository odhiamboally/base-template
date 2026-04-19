using BT.Application.Contracts.Interfaces.Services;
using BT.SharedKernel.Dtos.Common;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Contracts.Implementations.Services;

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
                _logger.LogInformation("Email sent successfully to {To}", sendEmailRequest.To);

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
                _logger.LogError("Failed to send email to {To}: {Errors}", sendEmailRequest.To, errorMessage);

                return AppResponse.Failure<SendEmailResponse>($"Failed to send email: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", sendEmailRequest?.To);
            throw;
        }
    }



}
