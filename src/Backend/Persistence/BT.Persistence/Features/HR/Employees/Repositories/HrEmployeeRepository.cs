using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Features.HR.DataContext;

namespace BT.Persistence.Features.HR.Employees.Repositories;

internal sealed class HrEmployeeRepository(HrDBContext context) : Repository<Employee>(context), IEmployeeRepository { }
