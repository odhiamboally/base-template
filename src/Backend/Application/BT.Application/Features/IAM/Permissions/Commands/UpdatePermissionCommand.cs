using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Permissions.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Features.IAM.Permissions.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.Permissions.Commands;


public sealed record UpdatePermissionCommand(Guid Id, UpdatePermissionRequest Request, string UserId)
    : IRequest<AppResponse<PermissionResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("permissions", Id.ToString())];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("permissions")];
}

