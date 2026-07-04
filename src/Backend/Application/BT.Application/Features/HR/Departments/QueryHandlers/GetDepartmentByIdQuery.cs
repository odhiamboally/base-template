using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Departments.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Departments.QueryHandlers;


public sealed record GetDepartmentByIdQuery(Guid Id, string UserId) : IRequest<AppResponse<DepartmentResponse>>, ICachableRequest
{
    public string CacheGroup => "departments";

    public string Discriminator => CacheKeys.Entity("departments", Id.ToString());

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

