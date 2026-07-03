using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.Shared.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Lookups.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.Shared.Lookups.QueryHandlers;



internal sealed class GetLookupCatalogTypesQueryHandler(
    ISharedUnitOfWork sharedUnitOfWork,
    ILogger<GetLookupCatalogTypesQueryHandler> logger)
    : IRequestHandler<GetLookupCatalogTypesQuery, AppResponse<IReadOnlyList<LookupCatalogTypeResponse>>>
{
    public async Task<AppResponse<IReadOnlyList<LookupCatalogTypeResponse>>> Handle(
        GetLookupCatalogTypesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var catalogTypes = await sharedUnitOfWork.LookupRepository
                .GetCatalogTypesAsync(cancellationToken)
                .ConfigureAwait(false);

            var response = catalogTypes
                .Select(type => new LookupCatalogTypeResponse(
                    type.Id,
                    type.Key,
                    type.Label,
                    type.Description,
                    type.IsActive))
                .ToList();

            return AppResponses.Success<IReadOnlyList<LookupCatalogTypeResponse>>(response);
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(GetLookupCatalogTypesQueryHandler), ex);
            throw;
        }
    }
}
