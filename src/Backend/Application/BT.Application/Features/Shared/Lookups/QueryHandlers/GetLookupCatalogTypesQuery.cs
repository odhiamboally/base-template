using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.Shared.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Lookups.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.Shared.Lookups.QueryHandlers;


public sealed record GetLookupCatalogTypesQuery : IRequest<AppResponse<IReadOnlyList<LookupCatalogTypeResponse>>>, ICachableRequest
{
    public string CacheGroup => "lookups";

    public string Discriminator => "catalog-types";

    public string? CacheUserId => null;

    public bool IsVersioned => false;
}

