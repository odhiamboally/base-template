using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Shared.Contracts.Common;

namespace BT.Infrastructure.Contracts.Implementations.Common;

internal sealed class NoOpIntegrationEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TIntegrationEvent : IIntegrationEvent
        => Task.CompletedTask;
}
