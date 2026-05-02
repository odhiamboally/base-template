namespace BT.Application.Features.HR.Employees.Contracts.Interfaces;

internal interface IEmployeeNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
