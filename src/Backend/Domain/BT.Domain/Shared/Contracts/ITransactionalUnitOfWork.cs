using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;

namespace BT.Domain.Shared.Contracts;

/// <summary>Common transactional operations and domain event management shared by all BC unit-of-work interfaces.</summary>
public interface ITransactionalUnitOfWork
{
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken);
    Task<TResult> ExecuteInTransactionWithRetryAsync<TResult>(Func<Task<TResult>> operation, int maxRetries = 3, int baseDelayMs = 50);
    Task<int> CompleteAsync(CancellationToken ct = default);
    IReadOnlyList<IDomainEvent> GetPendingDomainEvents();
    void ClearDomainEvents();
}
