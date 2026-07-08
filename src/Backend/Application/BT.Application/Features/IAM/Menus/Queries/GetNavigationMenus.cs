using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Menus.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Menus.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.Menus.Queries;



internal sealed class GetNavigationMenusQueryHandler(IIamUnitOfWork unitOfWork, ILogger<GetNavigationMenusQueryHandler> logger)
    : IRequestHandler<GetNavigationMenusQuery, AppResponse<IReadOnlyList<MenuResponse>>>
{
    public async Task<AppResponse<IReadOnlyList<MenuResponse>>> Handle(GetNavigationMenusQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var permissionSet = query.PermissionKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var menus = await unitOfWork.MenuRepository
                .ListAsync(
                    menus => menus
                        .Where(menu => menu.IsActive && menu.Placement == query.Placement)
                        .Where(menu => query.HasFullAccess || menu.RequiredPermissionKey == null || permissionSet.Contains(menu.RequiredPermissionKey))
                        .OrderBy(static menu => menu.DisplayOrder).ThenBy(static menu => menu.Title),
                    cancellationToken)
                .ConfigureAwait(false);

            return AppResponses.Success<IReadOnlyList<MenuResponse>>(menus.ToTree());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(GetNavigationMenusQueryHandler), ex);
            throw;
        }
    }
}
