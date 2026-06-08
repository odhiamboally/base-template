using BT.Domain.Features.HR.Departments.Entities;
using BT.SharedKernel.Features.HR.Departments.Dtos;

namespace BT.Application.Features.HR.Departments.Mappings;

public static class DepartmentMapping
{
    public static DepartmentResponse ToDepartmentResponse(this Department department)
    {
        ArgumentNullException.ThrowIfNull(department);

        return new DepartmentResponse(
            department.Id,
            department.Code,
            department.Name,
            department.Description,
            department.IsActive);
    }
}
