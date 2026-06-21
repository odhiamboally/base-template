using BT.SharedKernel.Features.Shared.Notifications.Dtos;
using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.Infrastructure.Configuration;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.Shared.Notifications.Contracts.Implementations.Services;

internal sealed class FluentMailService(IOptions<EmailSettings> options, ILogger<FluentMailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = options.Value;

    public async Task<AppResponse<SendEmailResponse>> SendEmailAsync(SendEmailRequest sendEmailRequest, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(sendEmailRequest);

            if (string.IsNullOrWhiteSpace(sendEmailRequest.To))
            {
                return AppResponse.Failure<SendEmailResponse>("Recipient email is required.");
            }

            if (!MailboxAddress.TryParse(sendEmailRequest.To, out var toAddress))
            {
                return AppResponse.Failure<SendEmailResponse>("The recipient email address is invalid.");
            }

            using var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.FromAddress));
            message.To.Add(toAddress);
            message.Subject = sendEmailRequest.Subject ?? string.Empty;
            message.Body = new BodyBuilder
            {
                HtmlBody = sendEmailRequest.Body ?? string.Empty
            }.ToMessageBody();

            using var smtpClient = new SmtpClient();
            var socketOptions = GetSocketOptions(_settings);

            await smtpClient.ConnectAsync(_settings.Host, _settings.Port, socketOptions, cancellationToken).ConfigureAwait(false);
            if (_settings.UseAuthentication && !string.IsNullOrWhiteSpace(_settings.Username))
            {
                await smtpClient.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken).ConfigureAwait(false);
            }

            var messageId = await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            await smtpClient.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

            ServiceLogDefinitions.LogEmailSent(logger, sendEmailRequest.To);

            return AppResponse.Success("Email sent successfully",
                new SendEmailResponse(
                    string.IsNullOrWhiteSpace(messageId) ? Guid.NewGuid().ToString() : messageId,
                    DateTimeOffset.UtcNow,
                    sendEmailRequest.To,
                    sendEmailRequest.Subject ?? string.Empty)
                );
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogErrorSendingEmail(logger, sendEmailRequest?.To ?? string.Empty, ex);
            return AppResponse.Failure<SendEmailResponse>("Email could not be sent. Please verify SMTP configuration and try again.");
        }
    }

    private static SecureSocketOptions GetSocketOptions(EmailSettings settings)
    {
        return !settings.EnableSsl
            ? SecureSocketOptions.None
            : settings.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;
    }
}
