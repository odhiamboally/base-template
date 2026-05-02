using BT.Domain.Shared.Contracts;
using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;

namespace BT.Domain.Features.Banking.Contracts;

public interface IBankingUnitOfWork : ITransactionalUnitOfWork
{
    ICustomerRepository CustomerRepository { get; }
}
