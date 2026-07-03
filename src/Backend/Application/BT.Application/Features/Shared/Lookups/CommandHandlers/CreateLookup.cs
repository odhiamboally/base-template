using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.Shared.Contracts;
using BT.Domain.Features.Shared.Lookups.Enums;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Lookups.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.Shared.Lookups.CommandHandlers;



internal sealed class CreateLookupCommandHandler(ISharedUnitOfWork unitOfWork, ILogger<CreateLookupCommandHandler> logger)
    : IRequestHandler<CreateLookupCommand, AppResponse<LookupResponse>>
{
    public async Task<AppResponse<LookupResponse>> Handle(CreateLookupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<LookupType>(command.LookupType, true, out var lookupType))
            {
                return AppResponses.Failure<LookupResponse>($"Invalid lookup type: {command.LookupType}");
            }

            var lookup = await unitOfWork.LookupRepository
                .CreateLookupAsync(lookupType, command.Request.Code, command.Request.Description, 0, cancellationToken)
                .ConfigureAwait(false);

            var saved = await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false) > 0;
            return saved
                ? AppResponses.Success("Lookup created.", new LookupResponse(lookup.Id, lookup.Code, lookup.Description, lookup.DisplayOrder))
                : AppResponses.Failure<LookupResponse>("Lookup create failed.");
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(CreateLookupCommandHandler), ex);
            throw;
        }
    }
}
