using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace BT.Application.Features.IAM.Users.Commands;

public sealed record UpdateUserRolesCommand(string UserId, UpdateUserRolesRequest Request, string UpdatedBy)
    : IRequest<AppResponse<UserRolesResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("user-roles", UserId)];
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("iam-admin")];
}
