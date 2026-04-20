using BT.Domain.Contracts.Interfaces.Repositories;

namespace BT.Domain.Contracts.Interfaces.Common;

public interface ISharedUnitOfWork
{
    ILookupRepository LookupRepository { get; }
    IEmailTemplateRepository EmailTemplateRepository { get; }
    IFailedMessageRepository FailedMessageRepository { get; }

    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken);
    Task<TResult> ExecuteInTransactionWithRetryAsync<TResult>(Func<Task<TResult>> operation, int maxRetries = 3, int baseDelayMs = 50);
    Task<int> CompleteAsync(CancellationToken ct = default);
    Task<int> CompleteWithEventsAsync(List<IIntegrationEvent>? appEvents = null, CancellationToken ct = default);
}
