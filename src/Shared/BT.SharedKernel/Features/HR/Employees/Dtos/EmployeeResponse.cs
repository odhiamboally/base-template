using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Features.HR.Employees.Dtos;

public record EmployeeResponse(
    Guid Id,
    string FullName,
    string EmployeeNumber,
    Guid DepartmentId
);
