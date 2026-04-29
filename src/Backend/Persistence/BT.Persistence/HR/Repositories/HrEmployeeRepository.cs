using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.HR.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.HR.DataContext;

namespace BT.Persistence.HR.Repositories;

internal sealed class HrEmployeeRepository(HrDbContext context) : Repository<Employee>(context), IEmployeeRepository { }
