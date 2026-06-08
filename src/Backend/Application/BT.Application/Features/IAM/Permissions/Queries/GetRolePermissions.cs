using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;

namespace BT.Application.Features.IAM.Permissions.Queries;

public sealed record GetRolePermissionsQuery(string RoleId, string UserId)
    : IRequest<AppResponse<RolePermissionsResponse>>, ICachableRequest
{
    public string CacheGroup => "iam-admin";

    public string Discriminator => CacheKeys.Entity("role-permissions", RoleId);

    public string? CacheUserId => null;

    public bool IsVersioned => true;

    public bool BypassCache => true;
}
