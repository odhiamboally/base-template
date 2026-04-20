using BT.Domain.Contracts.Interfaces.Repositories;

namespace BT.Domain.Contracts.Interfaces.Common;

public interface IBankingUnitOfWork : ITransactionalUnitOfWork
{
    ICustomerRepository CustomerRepository { get; }
}
