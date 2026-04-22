using BT.SharedKernel.Dtos.Common;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Entities;
using BT.SharedKernel.Dtos.Client;

using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.Clients.Commands;

/// <summary>
/// Same invalidation pattern as Update: remove the entity entry + bump the list version.
/// </summary>
public record DeleteClientCommand(Guid ClientId, string UserId) : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
    
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("clients", ClientId.ToString())];
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("clients", UserId)];
        
}

internal sealed class DeleteClientCommandHandler(IBankingUnitOfWork _bankingUow, ILogger<DeleteClientCommandHandler> _logger)
    : IRequestHandler<DeleteClientCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(DeleteClientCommand command, CancellationToken ct)
    {
        try
        {
            var customer = await _bankingUow.CustomerRepository.FindByIdAsync(command.ClientId, ct).ConfigureAwait(false);
            if (customer is null)
                return AppResponse.Failure<bool>($"Customer {command.ClientId} not found.");

            await _bankingUow.CustomerRepository.SoftDeleteAsync(command.ClientId, ct).ConfigureAwait(false);

            var saved = await _bankingUow.CompleteAsync(ct).ConfigureAwait(false) > 0;
            if (!saved)
                return AppResponse.Failure<bool>("Failed to delete customer.");
            return AppResponse.Success(true);
        }
        catch (Exception ex)
        {
            LogDefinitions.LogCustomerDeleteFailed(_logger, command.ClientId, ex);
            throw;
        }
        
    }
}
