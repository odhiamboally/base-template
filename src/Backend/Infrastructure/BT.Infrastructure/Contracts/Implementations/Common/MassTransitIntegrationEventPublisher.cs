using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using MassTransit;

namespace BT.Infrastructure.Contracts.Implementations.Common;

internal sealed class MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
{
    public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        where TIntegrationEvent : IIntegrationEvent
        => publishEndpoint.Publish(integrationEvent, cancellationToken);
}
