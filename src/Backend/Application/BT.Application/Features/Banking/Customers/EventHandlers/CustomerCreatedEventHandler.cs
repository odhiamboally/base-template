using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Extensions;
using BT.Application.IntegrationEvents;
using BT.Application.Utilities;
using BT.Domain.Events;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.Banking.Customers.EventHandlers;

public class CustomerCreatedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher,
    ILogger<CustomerCreatedEventHandler> _logger) 
    : INotificationHandler<CustomerCreatedEvent>
{
    public async Task Handle(CustomerCreatedEvent evt, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(evt, nameof(evt));

            var integrationEvent = new CustomerCreatedIntegrationEvent(
                evt.ClientId, 
                evt.ClientNumber, 
                evt.ClientName, 
                evt.Email, 
                evt.ClientType.ToDisplayString()
                
            );
            
            await integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);

            LogDefinitions.LogCustomerCreatedIntegrationPublished(_logger, evt.ClientId);
        }
        catch (Exception ex)
        {
            LogDefinitions.LogCustomerCreatedDomainEventHandlerError(_logger, evt?.Email ?? string.Empty, ex);
            throw;
        }
    }
}
