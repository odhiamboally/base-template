using BT.Domain.Features.HR.Departments.Contracts.Repositories;
using BT.Domain.Features.HR.Departments.Entities;
using BT.Persistence.Common.Repositories;
using BT.Persistence.Features.HR.DataContext;

namespace BT.Persistence.Features.HR.Departments.Repositories;

internal sealed class HrDepartmentRepository(HrDBContext context) : Repository<Department>(context), IDepartmentRepository
{
}
