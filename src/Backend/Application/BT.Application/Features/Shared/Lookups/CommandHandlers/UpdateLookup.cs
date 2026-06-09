using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.Shared.Contracts;
using BT.Domain.Features.Shared.Lookups.Enums;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Lookups.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.Shared.Lookups.CommandHandlers;

public sealed record UpdateLookupCommand(string LookupType, int Id, UpdateLookupRequest Request, string UserId)
    : IRequest<AppResponse<LookupResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("lookups", $"{LookupType}:{Id}")];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("lookups")];
}

internal sealed class UpdateLookupCommandHandler(ISharedUnitOfWork unitOfWork, ILogger<UpdateLookupCommandHandler> logger)
    : IRequestHandler<UpdateLookupCommand, AppResponse<LookupResponse>>
{
    public async Task<AppResponse<LookupResponse>> Handle(UpdateLookupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<LookupType>(command.LookupType, true, out var lookupType))
            {
                return AppResponse.Failure<LookupResponse>($"Invalid lookup type: {command.LookupType}");
            }

            var lookup = await unitOfWork.LookupRepository
                .UpdateLookupAsync(lookupType, command.Id, command.Request.Code, command.Request.Description, 0, cancellationToken)
                .ConfigureAwait(false);

            var saved = await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false) > 0;
            return saved
                ? AppResponse.Success("Lookup updated.", new LookupResponse(lookup.Id, lookup.Code, lookup.Description, lookup.DisplayOrder))
                : AppResponse.Failure<LookupResponse>("Lookup update failed.");
        }
        catch (KeyNotFoundException ex)
        {
            return AppResponse.Failure<LookupResponse>(ex.Message);
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(UpdateLookupCommandHandler), ex);
            throw;
        }
    }
}
