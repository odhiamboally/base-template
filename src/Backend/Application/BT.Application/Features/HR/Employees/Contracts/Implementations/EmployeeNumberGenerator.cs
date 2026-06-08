using BT.Application.Features.HR.Employees.Contracts.Interfaces;
using BT.Domain.Features.HR.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.HR.Employees.Contracts.Implementations;


internal sealed class EmployeeNumberGenerator(IHrUnitOfWork _hrUnitOfWork) : IEmployeeNumberGenerator
{
    public async Task<string> GenerateAsync(Guid departmentId, CancellationToken ct = default)
    {
        var department = await _hrUnitOfWork.DepartmentRepository.FindByIdAsync(departmentId, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Department is required before generating an employee number.");

        var prefix = department.Code;
        var totalCount = await _hrUnitOfWork.EmployeeRepository
            .FindByCondition(employee => employee.DepartmentId == departmentId)
            .CountAsync(ct)
            .ConfigureAwait(false);
        var sequence = totalCount + 1;
        return $"{prefix}-{DateTime.UtcNow.Year}-{sequence:D4}";
    }
}
