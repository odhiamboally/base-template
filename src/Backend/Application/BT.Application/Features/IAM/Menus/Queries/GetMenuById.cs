using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Menus.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Menus.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.Menus.Queries;

public sealed record GetMenuByIdQuery(Guid Id, string UserId) : IRequest<AppResponse<MenuResponse>>, ICachableRequest
{
    public string CacheGroup => "menus";
    public string Discriminator => CacheKeys.Entity("menus", Id.ToString());
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}

internal sealed class GetMenuByIdQueryHandler(IIamUnitOfWork unitOfWork, ILogger<GetMenuByIdQueryHandler> logger)
    : IRequestHandler<GetMenuByIdQuery, AppResponse<MenuResponse>>
{
    public async Task<AppResponse<MenuResponse>> Handle(GetMenuByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var menu = await unitOfWork.MenuRepository.FindByIdAsync(query.Id, cancellationToken).ConfigureAwait(false);
            return menu is null
                ? AppResponse.Failure<MenuResponse>($"Menu {query.Id} not found.")
                : AppResponse.Success(menu.ToMenuResponse());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(GetMenuByIdQueryHandler), ex);
            throw;
        }
    }
}
