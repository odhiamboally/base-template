using BT.Domain.HR.Entities;
using BT.SharedKernel.Dtos.Employees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace BT.Application.Mappings;

public static class EmployeeMapping
{
    public static Employee ToEntity(this CreateEmployeeRequest request, string createdBy) 
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        return Employee.Create(
            request.EmployeeNumber,
            request.Email,
            request.FirstName,
            request.LastName,
            request.IdNumber,
            request.PhoneNumber,
            request.DepartmentId,
            request.ManagerId ?? Guid.Empty,
            createdBy
        );
    }
        
    public static EmployeeResponse ToEmployeeResponse(this Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee, nameof(employee));

        return new(
            employee.Id,
            $"{employee.FirstName} {employee.LastName}",
            employee.EmployeeNumber,
            employee.DepartmentId
        );
    }
       
    public static Expression<Func<Employee, EmployeeResponse>> AsResponse => employee =>
        new EmployeeResponse(
            employee.Id,
            $"{employee.FirstName} {employee.LastName}",
            employee.EmployeeNumber,
            employee.DepartmentId
        );

    public static List<EmployeeResponse> ToEmployeeResponseList(this Collection<Employee> employees) =>
        [.. employees.Select(e => e.ToEmployeeResponse())];

    public static List<Employee> ToEntityList(this Collection<CreateEmployeeRequest> requests, string createdBy) =>
        [.. requests.Select(r => r.ToEntity(createdBy))];

}
