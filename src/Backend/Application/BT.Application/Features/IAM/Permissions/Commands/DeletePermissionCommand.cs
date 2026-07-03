using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Contracts;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.Permissions.Commands;


public sealed record DeletePermissionCommand(Guid Id, string UserId)
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("permissions", Id.ToString())];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("permissions")];
}

