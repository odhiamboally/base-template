using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.HR.DataContext;

namespace BT.Persistence.HR.Repositories;

internal sealed class HrEmployeeRepository(HrDbContext context) : Repository<Employee>(context), IEmployeeRepository { }
