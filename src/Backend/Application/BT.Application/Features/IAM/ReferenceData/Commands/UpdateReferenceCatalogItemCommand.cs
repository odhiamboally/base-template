using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.ReferenceData.Mappings;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.Domain.Features.IAM.Contracts;
using BT.SharedKernel.Features.IAM.ReferenceData.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.ReferenceData.Commands;


public sealed record UpdateReferenceCatalogItemCommand(string CatalogType, Guid Id, ReferenceCatalogItemRequest Request, string UserId)
    : IRequest<AppResponse<ReferenceCatalogItemResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("iam-reference-data")];
}

