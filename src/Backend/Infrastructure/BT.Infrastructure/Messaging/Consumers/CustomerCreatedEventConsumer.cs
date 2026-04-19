using BT.Application.Extensions;
using BT.Application.IntegrationEvents;
using BT.Infrastructure.Logging;
using MassTransit;
using Microsoft.Extensions.Logging;
using BT.SharedKernel.Dtos.Common;

namespace BT.Infrastructure.Messaging.Consumers;

public sealed class CustomerCreatedEventConsumer(ILogger<CustomerCreatedEventConsumer> logger) : IConsumer<CustomerCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CustomerCreatedIntegrationEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var evt = context.Message;

        try
        {
            var sendWelcomeEmailRequest = new SendWelcomeEmailRequest(evt.ClientId, evt.ClientNumber, evt.ClientName, evt.Email, evt.ClientType);
            await context.Publish(sendWelcomeEmailRequest, context.CancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            MessageBusLogDefinitions.LogClientCreatedIntegrationConsumeError(logger, evt.ClientId, ex);
            throw;
        }
    }
}
