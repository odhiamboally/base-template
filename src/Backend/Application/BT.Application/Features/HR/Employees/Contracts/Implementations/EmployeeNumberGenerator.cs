using BT.Application.Features.HR.Employees.Contracts.Interfaces;
using BT.Domain.Features.HR.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.HR.Employees.Contracts.Implementations;


internal sealed class EmployeeNumberGenerator(IHrUnitOfWork _hrUnitOfWork) : IEmployeeNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var prefix = "EMP";
        var totalCount = await _hrUnitOfWork.EmployeeRepository.CountAsync(ct).ConfigureAwait(false);
        var sequence = totalCount + 1;
        return $"{prefix}-{sequence:D5}"; // EMP-00001
    }
}
