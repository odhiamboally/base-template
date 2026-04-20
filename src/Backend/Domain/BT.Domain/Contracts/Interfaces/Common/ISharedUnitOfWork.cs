using BT.Domain.Contracts.Interfaces.Repositories;

namespace BT.Domain.Contracts.Interfaces.Common;

public interface ISharedUnitOfWork : ITransactionalUnitOfWork
{
    ILookupRepository LookupRepository { get; }
    IEmailTemplateRepository EmailTemplateRepository { get; }
    IFailedMessageRepository FailedMessageRepository { get; }

    Task<int> CompleteWithEventsAsync(List<IIntegrationEvent>? appEvents = null, CancellationToken ct = default);
}
