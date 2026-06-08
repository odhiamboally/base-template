using BT.Domain.Shared.Contracts;
using BT.Domain.Features.HR.Departments.Contracts.Repositories;
using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;

namespace BT.Domain.Features.HR.Contracts;

public interface IHrUnitOfWork : ITransactionalUnitOfWork
{
    IDepartmentRepository DepartmentRepository { get; }
    IEmployeeRepository EmployeeRepository { get; }
}
