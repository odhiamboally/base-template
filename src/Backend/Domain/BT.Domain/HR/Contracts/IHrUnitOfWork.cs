using BT.Domain.Shared.Contracts;
using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;

namespace BT.Domain.HR.Contracts;

public interface IHrUnitOfWork : ITransactionalUnitOfWork
{
    IEmployeeRepository EmployeeRepository { get; }
}
