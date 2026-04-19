using BT.Domain.Contracts.Interfaces.Common;

namespace BT.Application.Contracts.Interfaces.Common;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        where TIntegrationEvent : IIntegrationEvent;
}
