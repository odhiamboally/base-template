using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Extensions;
using BT.Application.IntegrationEvents;
using BT.Application.Mappings;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Entities;
using BT.Domain.Enums;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BT.Infrastructure.Messaging.Consumers;

public sealed class SendWelcomeEmailConsumer(IServiceManager serviceManager, IUnitOfWork unitOfWork, ILogger<SendWelcomeEmailConsumer> logger) 
    : IConsumer<SendWelcomeEmailRequest>
{
    public async Task Consume(ConsumeContext<SendWelcomeEmailRequest> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var request = context.Message;
        var retryCount = context.GetRetryCount();

        if (context.Headers.TryGetHeader("MT-Redelivery-Count", out var retryCountValue) &&
            retryCountValue is not null)
        {
            _ = int.TryParse(retryCountValue.ToString(), out retryCount);
        }

        var messageId = context.MessageId?.ToString() ?? Guid.CreateVersion7().ToString();

        try
        {
            var customer = await unitOfWork.CustomerRepository.FindByIdAsync(request.ClientId).ConfigureAwait(false);
            if (customer == null)
            {
                return;
            }

            // ToDo: Create Category for Client and use it to determine the email template instead of using ClientType
            var emailTemplate = request.ClientType.ToEnum<CustomerType>() switch
            {
                CustomerType.Institutional => EmailTemplateType.Institutional,
                CustomerType.Corporate => EmailTemplateType.Corporate,
                CustomerType.SmallMediumEnterprise => EmailTemplateType.SmallMediumEnterprise,
                CustomerType.Individual => throw new NotImplementedException(),
                CustomerType.Enterprise => throw new NotImplementedException(),
                _ => EmailTemplateType.StandardWelcome
            };

            var dbEmailTemplate = await unitOfWork.EmailTemplateRepository
                .FindByCondition(t => t.Name == emailTemplate.ToTemplateName())
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (dbEmailTemplate == null)
            {
                MessageBusLogDefinitions.LogEmailTemplateNotFound(logger, emailTemplate.ToTemplateName());
                return;
            }

            var composeEmailResponse = await serviceManager.MailComposer.ComposeClientCreatedAsync(request, emailTemplate).ConfigureAwait(false);
            if (composeEmailResponse is null || !composeEmailResponse.Successful)
            {
                return;
            }

            var sendEmailRequest = new SendEmailRequest
            {
                To = customer.Address.Email,
                Subject = composeEmailResponse.Data?.Subject ?? string.Empty,
                Body = composeEmailResponse.Data?.Body ?? string.Empty
            };

            await context.Publish(sendEmailRequest).ConfigureAwait(false);
        }
        catch (Exception ex) when (retryCount >= 4)
        {
            MessageBusLogDefinitions.LogPermanentConsumerFailure(logger, retryCount + 1, ex);

            var failedMessage = new FailedMessage
            {
                Id = Guid.CreateVersion7(),
                MessageId = messageId,
                MessageType = nameof(CustomerCreatedIntegrationEvent),
                EntityId = request.ClientId.ToString(),
                Payload = JsonSerializer.Serialize(request),
                ErrorMessage = ex.Message,
                ErrorStackTrace = ex.StackTrace,
                RetryCount = retryCount + 1,
                FailedAt = DateTimeOffset.UtcNow,
                Status = FailedMessageStatus.Permanent,
                CreatedBy = nameof(SendWelcomeEmailConsumer)
            };

            await unitOfWork.FailedMessageRepository.CreateAsync(failedMessage, context.CancellationToken).ConfigureAwait(false);
            await unitOfWork.CompleteAsync(context.CancellationToken).ConfigureAwait(false);
            return;
        }
        catch (Exception ex)
        {
            MessageBusLogDefinitions.LogTemporaryConsumerFailure(logger, retryCount + 1, ex);
            throw;
        }
    }
}
