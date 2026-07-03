using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.Shared.Contracts;
using BT.Domain.Features.Shared.Lookups.Enums;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.Shared.Lookups.CommandHandlers;



internal sealed class DeleteLookupCommandHandler(ISharedUnitOfWork unitOfWork, ILogger<DeleteLookupCommandHandler> logger)
    : IRequestHandler<DeleteLookupCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(DeleteLookupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<LookupType>(command.LookupType, true, out var lookupType))
            {
                return AppResponses.Failure<bool>($"Invalid lookup type: {command.LookupType}");
            }

            await unitOfWork.LookupRepository
                .SoftDeleteLookupAsync(lookupType, command.Id, command.UserId, cancellationToken)
                .ConfigureAwait(false);

            var saved = await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false) > 0;
            return saved
                ? AppResponses.Success("Lookup deleted.", true)
                : AppResponses.Failure<bool>("Lookup delete failed.");
        }
        catch (KeyNotFoundException ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(DeleteLookupCommandHandler), ex);
            return AppResponses.Failure<bool>("The lookup record could not be found.");
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(DeleteLookupCommandHandler), ex);
            throw;
        }
    }
}
