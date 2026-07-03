using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace BT.Application.Features.IAM.Users.Queries;

public sealed record GetUserRolesQuery(string UserId, string RequestedBy)
    : IRequest<AppResponse<UserRolesResponse>>, ICachableRequest
{
    public string CacheGroup => "iam-admin";
    public string Discriminator => CacheKeys.Entity("user-roles", UserId);
    public string? CacheUserId => null;
    public bool IsVersioned => true;
    public bool BypassCache => true;
}
