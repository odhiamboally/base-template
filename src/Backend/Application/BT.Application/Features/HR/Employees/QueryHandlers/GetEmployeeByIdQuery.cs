using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Employees.Dtos;
using MediatR;

namespace BT.Application.Features.HR.Employees.QueryHandlers;


public sealed record GetEmployeeByIdQuery(Guid Id, string UserId)
    : IRequest<AppResponse<EmployeeResponse>>, ICachableRequest
{
    public string CacheGroup => "employees";

    public string Discriminator => CacheKeys.Entity("employees", Id.ToString());

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

