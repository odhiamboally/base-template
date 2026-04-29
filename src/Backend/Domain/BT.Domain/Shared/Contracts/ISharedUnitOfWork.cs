using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;

namespace BT.Domain.Shared.Contracts;

public interface ISharedUnitOfWork : ITransactionalUnitOfWork
{
    ILookupRepository LookupRepository { get; }
    IEmailTemplateRepository EmailTemplateRepository { get; }
    IFailedMessageRepository FailedMessageRepository { get; }

    Task<int> CompleteWithEventsAsync(List<IIntegrationEvent>? appEvents = null, CancellationToken ct = default);
}
