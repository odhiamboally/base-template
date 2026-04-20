using BT.Domain.Contracts.Interfaces.Repositories;

namespace BT.Domain.Contracts.Interfaces.Common;

/// <summary>Backward-compat shim. Prefer the BC-scoped interfaces instead.</summary>
[Obsolete("Use IIamUnitOfWork, IHrUnitOfWork, IBankingUnitOfWork, or ISharedUnitOfWork instead.")]
public interface IUnitOfWork : IIamUnitOfWork, IHrUnitOfWork, IBankingUnitOfWork, ISharedUnitOfWork
{
    IReadOnlyList<IDomainEvent> GetPendingDomainEvents();
    void ClearDomainEvents();
}
