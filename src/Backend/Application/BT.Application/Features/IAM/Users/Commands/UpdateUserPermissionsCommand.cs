using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace BT.Application.Features.IAM.Users.Commands;

public sealed record UpdateUserPermissionsCommand(string UserId, UpdateUserPermissionsRequest Request, string UpdatedBy)
    : IRequest<AppResponse<UserPermissionsResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("user-permissions", UserId)];
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("iam-admin")];
}
