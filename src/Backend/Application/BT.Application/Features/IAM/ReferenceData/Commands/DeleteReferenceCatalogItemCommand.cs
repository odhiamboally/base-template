using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.Domain.Features.IAM.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.ReferenceData.Commands;


public sealed record DeleteReferenceCatalogItemCommand(string CatalogType, Guid Id, string UserId)
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("iam-reference-data")];
}

