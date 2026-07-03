using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.Shared.Contracts;
using BT.Domain.Features.Shared.Lookups.Enums;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.Shared.Lookups.CommandHandlers;


public sealed record DeleteLookupCommand(string LookupType, int Id, string UserId)
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("lookups", $"{LookupType}:{Id}")];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("lookups")];
}

