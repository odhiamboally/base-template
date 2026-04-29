using BT.Domain.Shared.Contracts;
using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;

namespace BT.Domain.Banking.Contracts;

public interface IBankingUnitOfWork : ITransactionalUnitOfWork
{
    ICustomerRepository CustomerRepository { get; }
}
