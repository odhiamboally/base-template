using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Departments.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.HR.Departments.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Departments.CommandHandlers;


public sealed record CreateDepartmentCommand(CreateDepartmentRequest Request, string UserId)
    : IRequest<AppResponse<DepartmentResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("departments")];
}

