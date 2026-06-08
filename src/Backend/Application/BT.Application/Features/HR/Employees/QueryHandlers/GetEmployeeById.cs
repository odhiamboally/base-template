using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Employees.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BT.Application.Features.HR.Employees.QueryHandlers;

public sealed record GetEmployeeByIdQuery(Guid Id, string UserId)
    : IRequest<AppResponse<EmployeeResponse>>, ICachableRequest
{
    public string CacheGroup => "employees";

    public string Discriminator => CacheKeys.Entity("employees", Id.ToString());

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

internal sealed class GetEmployeeByIdQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetEmployeeByIdQuery, AppResponse<EmployeeResponse>>
{
    public async Task<AppResponse<EmployeeResponse>> Handle(GetEmployeeByIdQuery query, CancellationToken cancellationToken)
    {
        var employee = await unitOfWork.EmployeeRepository.FindByIdAsync(query.Id, cancellationToken).ConfigureAwait(false);
        var departmentName = employee is null
            ? string.Empty
            : await unitOfWork.DepartmentRepository
                .FindByCondition(department => department.Id == employee.DepartmentId)
                .Select(static department => department.Name)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false) ?? string.Empty;
        var managerName = employee?.ManagerId is null
            ? string.Empty
            : await unitOfWork.EmployeeRepository
                .FindByCondition(manager => manager.Id == employee.ManagerId.Value)
                .Select(static manager => $"{manager.FirstName} {manager.LastName}")
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false) ?? string.Empty;

        return employee is null
            ? AppResponse.Failure<EmployeeResponse>($"Employee {query.Id} not found.")
            : AppResponse.Success("Employee loaded.", employee.ToEmployeeResponse(departmentName, managerName));
    }
}
