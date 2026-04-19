using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Interfaces.Common;
using MassTransit;

namespace BT.Infrastructure.Contracts.Implementations.Common;

internal sealed class MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
{
    public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        where TIntegrationEvent : IIntegrationEvent
        => publishEndpoint.Publish(integrationEvent, cancellationToken);
}
