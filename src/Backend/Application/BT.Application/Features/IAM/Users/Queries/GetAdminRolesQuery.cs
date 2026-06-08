using BT.Application.Contracts.Interfaces.Common;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace BT.Application.Features.IAM.Users.Queries;

public sealed record GetAdminRolesQuery : IRequest<AppResponse<IReadOnlyList<AdminRoleListResponse>>>, ICachableRequest
{
    public string CacheGroup => "iam-admin";
    public string Discriminator => "roles";
    public string? CacheUserId => null;
    public bool IsVersioned => true;
    public bool BypassCache => true;
}
