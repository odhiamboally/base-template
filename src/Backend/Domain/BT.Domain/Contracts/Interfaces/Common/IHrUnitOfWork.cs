using BT.Domain.Contracts.Interfaces.Repositories;

namespace BT.Domain.Contracts.Interfaces.Common;

public interface IHrUnitOfWork : ITransactionalUnitOfWork
{
    IEmployeeRepository EmployeeRepository { get; }
}
