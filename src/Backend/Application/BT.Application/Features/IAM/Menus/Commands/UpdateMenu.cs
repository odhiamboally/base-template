using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Menus.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Features.IAM.Menus.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Menus.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.Menus.Commands;

public sealed record UpdateMenuCommand(Guid Id, UpdateMenuRequest Request, string UserId)
    : IRequest<AppResponse<MenuResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("menus", Id.ToString())];
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("menus")];
}

internal sealed class UpdateMenuCommandHandler(IIamUnitOfWork unitOfWork, ILogger<UpdateMenuCommandHandler> logger)
    : IRequestHandler<UpdateMenuCommand, AppResponse<MenuResponse>>
{
    public async Task<AppResponse<MenuResponse>> Handle(UpdateMenuCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var request = command.Request;
            var catalogError = await ValidateCatalogAsync(unitOfWork, request.Placement, request.Icon, request.Url, request.RequiredPermissionKey, cancellationToken)
                .ConfigureAwait(false);
            if (catalogError is not null)
            {
                return AppResponse.Failure<MenuResponse>(catalogError);
            }

            var menu = await unitOfWork.MenuRepository.FindByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
            if (menu is null)
            {
                return AppResponse.Failure<MenuResponse>($"Menu {command.Id} not found.");
            }

            if (request.ParentId == command.Id)
            {
                return AppResponse.Failure<MenuResponse>("A menu cannot be its own parent.");
            }

            if (request.ParentId.HasValue)
            {
                var parent = await unitOfWork.MenuRepository.FindByIdAsync(request.ParentId.Value, cancellationToken).ConfigureAwait(false);
                if (parent is null)
                {
                    return AppResponse.Failure<MenuResponse>("Parent menu not found.");
                }

                if (!string.Equals(parent.Placement, request.Placement, StringComparison.OrdinalIgnoreCase)
                    || parent.DepartmentId != request.DepartmentId)
                {
                    return AppResponse.Failure<MenuResponse>("Parent menu must use the same placement and department scope.");
                }
            }

            var key = Slugify(request.Title);
            var draft = MenuItem.Create(request.ParentId, request.DepartmentId, key, request.Title, request.Description, request.Url, request.Icon, request.Placement, request.RequiredPermissionKey, command.UserId);
            var duplicate = await unitOfWork.MenuRepository
                .FindByCondition(existing => existing.Id != command.Id && existing.Key == draft.Key)
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);

            if (duplicate)
            {
                return AppResponse.Failure<MenuResponse>($"Menu key {draft.Key} already exists.");
            }

            menu.Update(request.ParentId, request.DepartmentId, key, request.Title, request.Description, request.Url, request.Icon, request.Placement, request.RequiredPermissionKey, request.IsActive, command.UserId);
            await unitOfWork.MenuRepository.UpdateAsync(menu).ConfigureAwait(false);
            var saved = await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false) > 0;

            return saved ? AppResponse.Success("Menu updated.", menu.ToMenuResponse()) : AppResponse.Failure<MenuResponse>("Menu update failed.");
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(UpdateMenuCommandHandler), ex);
            throw;
        }
    }

    private static string Slugify(string value)
        => string.Join('-', value.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static async Task<string?> ValidateCatalogAsync(
        IIamUnitOfWork unitOfWork,
        string placement,
        string icon,
        string url,
        string? requiredPermissionKey,
        CancellationToken cancellationToken)
    {
        var normalizedPlacement = placement.Trim();

        var placementExists = await unitOfWork.MenuPlacementRepository
            .FindByCondition(item => item.IsActive && item.Key == normalizedPlacement)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!placementExists)
        {
            return $"Menu placement '{placement}' is not registered.";
        }

        var iconExists = await unitOfWork.MenuIconRepository
            .FindByCondition(item => item.IsActive && item.Key == icon.Trim())
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!iconExists)
        {
            return $"Menu icon '{icon}' is not registered.";
        }

        var routeExists = string.IsNullOrWhiteSpace(url)
            || await unitOfWork.MenuRouteRepository
                .FindByCondition(item => item.IsActive && item.Url == url.Trim() && item.PlacementKey == normalizedPlacement)
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);

        if (!routeExists)
        {
            return $"Menu route '{url}' is not registered for placement '{placement}'.";
        }

        if (string.IsNullOrWhiteSpace(requiredPermissionKey))
        {
            return null;
        }

        var normalizedPermissionKey = requiredPermissionKey.Trim().ToLowerInvariant();
        var permissionExists = await unitOfWork.PermissionRepository
            .FindByCondition(item => item.IsActive && item.Key == normalizedPermissionKey)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        return permissionExists ? null : $"Required permission '{requiredPermissionKey}' is not registered.";
    }
}
