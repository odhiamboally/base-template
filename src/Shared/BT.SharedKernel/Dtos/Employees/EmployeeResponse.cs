using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Employees;

public record EmployeeResponse(
    Guid Id,
    string FullName,
    string EmployeeNumber,
    Guid DepartmentId
);
