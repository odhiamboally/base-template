using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.ReferenceData.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.ReferenceData.Queries;

public sealed record GetIamReferenceDataQuery : IRequest<AppResponse<IamReferenceDataResponse>>, ICachableRequest
{
    public string CacheGroup => "iam-reference-data";
    public string Discriminator => CacheKeys.Discriminator("all");
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}

internal sealed class GetIamReferenceDataQueryHandler(IIamUnitOfWork unitOfWork, ILogger<GetIamReferenceDataQueryHandler> logger)
    : IRequestHandler<GetIamReferenceDataQuery, AppResponse<IamReferenceDataResponse>>
{
    public async Task<AppResponse<IamReferenceDataResponse>> Handle(GetIamReferenceDataQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var contexts = await unitOfWork.PermissionContextRepository.FindByCondition(static item => item.IsActive)
                .OrderBy(static item => item.Label)
                .Select(static item => new CatalogOptionResponse(item.Key, item.Label, item.Description))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var resources = await unitOfWork.PermissionResourceRepository.FindByCondition(static item => item.IsActive)
                .OrderBy(static item => item.ContextKey)
                .ThenBy(static item => item.Label)
                .Select(static item => new CatalogOptionResponse(item.Key, item.Label, item.Description, item.ContextKey))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var actions = await unitOfWork.PermissionActionRepository.FindByCondition(static item => item.IsActive)
                .OrderBy(static item => item.Label)
                .Select(static item => new CatalogOptionResponse(item.Key, item.Label, item.Description))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var placements = await unitOfWork.MenuPlacementRepository.FindByCondition(static item => item.IsActive)
                .OrderBy(static item => item.Label)
                .Select(static item => new CatalogOptionResponse(item.Key, item.Label, item.Description))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var icons = await unitOfWork.MenuIconRepository.FindByCondition(static item => item.IsActive)
                .OrderBy(static item => item.Label)
                .Select(static item => new CatalogOptionResponse(item.Key, item.Label, item.Description))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var routes = await unitOfWork.MenuRouteRepository.FindByCondition(static item => item.IsActive)
                .OrderBy(static item => item.PlacementKey)
                .ThenBy(static item => item.Label)
                .Select(static item => new CatalogOptionResponse(item.Url, item.Label, item.Description, item.PlacementKey))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var response = new IamReferenceDataResponse(contexts, resources, actions, placements, icons, routes);
            return AppResponse.Success("IAM reference data loaded.", response);
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(GetIamReferenceDataQueryHandler), ex);
            throw;
        }
    }
}
