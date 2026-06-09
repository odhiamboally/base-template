using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Contracts;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.Permissions.Commands;

public sealed record DeletePermissionCommand(Guid Id, string UserId)
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("permissions", Id.ToString())];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("permissions")];
}

internal sealed class DeletePermissionCommandHandler(IIamUnitOfWork unitOfWork, ILogger<DeletePermissionCommandHandler> logger)
    : IRequestHandler<DeletePermissionCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(DeletePermissionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var permission = await unitOfWork.PermissionRepository.FindByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
            if (permission is null)
            {
                return AppResponse.Failure<bool>($"Permission {command.Id} not found.");
            }

            permission.MarkAsDeleted(command.UserId);
            await unitOfWork.PermissionRepository.UpdateAsync(permission).ConfigureAwait(false);
            var saved = await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false) > 0;

            return saved
                ? AppResponse.Success("Permission deleted.", true)
                : AppResponse.Failure<bool>("Permission delete failed.");
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(DeletePermissionCommandHandler), ex);
            throw;
        }
    }
}
