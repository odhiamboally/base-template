using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Contracts;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.Menus.Commands;

public sealed record DeleteMenuCommand(Guid Id, string UserId)
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("menus", Id.ToString())];
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("menus")];
}

internal sealed class DeleteMenuCommandHandler(IIamUnitOfWork unitOfWork, ILogger<DeleteMenuCommandHandler> logger)
    : IRequestHandler<DeleteMenuCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(DeleteMenuCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var menu = await unitOfWork.MenuRepository.FindByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
            if (menu is null)
            {
                return AppResponse.Failure<bool>($"Menu {command.Id} not found.");
            }

            var hasChildren = await unitOfWork.MenuRepository
                .AnyAsync(child => child.ParentId == command.Id, cancellationToken)
                .ConfigureAwait(false);

            if (hasChildren)
            {
                return AppResponse.Failure<bool>("This menu has child menu items. Move or delete the children first.");
            }

            menu.MarkAsDeleted(command.UserId);
            await unitOfWork.MenuRepository.UpdateAsync(menu).ConfigureAwait(false);
            var saved = await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false) > 0;

            return saved ? AppResponse.Success("Menu deleted.", true) : AppResponse.Failure<bool>("Menu delete failed.");
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(DeleteMenuCommandHandler), ex);
            throw;
        }
    }
}
