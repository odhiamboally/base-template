using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;

namespace BT.Application.Features.IAM.Permissions.Commands;

public sealed record UpdateRolePermissionsCommand(string RoleId, UpdateRolePermissionsRequest Request, string UserId)
    : IRequest<AppResponse<RolePermissionsResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("role-permissions", RoleId)];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("iam-admin")];
}
