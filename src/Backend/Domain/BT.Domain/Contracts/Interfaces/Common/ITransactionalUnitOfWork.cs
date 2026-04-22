using BT.Domain.Contracts.Interfaces.Repositories;

namespace BT.Domain.Contracts.Interfaces.Common;

/// <summary>Common transactional operations and domain event management shared by all BC unit-of-work interfaces.</summary>
public interface ITransactionalUnitOfWork
{
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken);
    Task<TResult> ExecuteInTransactionWithRetryAsync<TResult>(Func<Task<TResult>> operation, int maxRetries = 3, int baseDelayMs = 50);
    Task<int> CompleteAsync(CancellationToken ct = default);
    IReadOnlyList<IDomainEvent> GetPendingDomainEvents();
    void ClearDomainEvents();
}
