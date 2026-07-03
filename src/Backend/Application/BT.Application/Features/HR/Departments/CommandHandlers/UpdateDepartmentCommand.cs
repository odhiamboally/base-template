using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Departments.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Departments.CommandHandlers;


public sealed record UpdateDepartmentCommand(Guid Id, UpdateDepartmentRequest Request, string UserId)
    : IRequest<AppResponse<DepartmentResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("departments", Id.ToString())];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("departments"), CacheKeys.GroupVersion("employees")];
}

