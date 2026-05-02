using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.Shared.EmailTemplates.Contracts.Repositories;
using BT.Domain.Features.Shared.FailedMessages.Contracts.Repositories;
using BT.Domain.Features.Shared.Lookups.Contracts.Repositories;

namespace BT.Domain.Features.Shared.Contracts;

public interface ISharedUnitOfWork : ITransactionalUnitOfWork
{
    ILookupRepository LookupRepository { get; }
    IEmailTemplateRepository EmailTemplateRepository { get; }
    IFailedMessageRepository FailedMessageRepository { get; }

    Task<int> CompleteWithEventsAsync(List<IIntegrationEvent>? appEvents = null, CancellationToken ct = default);
}
