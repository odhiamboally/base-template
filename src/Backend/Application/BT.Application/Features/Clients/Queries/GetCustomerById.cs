using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Mappings;
using BT.Application.Utilities;
using BT.Domain.Contracts.Interfaces.Common;
using BT.SharedKernel.Dtos.Client;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.Clients.Queries;

/// <summary>
/// Fetches a single client by ID.
///
/// Cache strategy — NON-VERSIONED entity entry:
///   Key:  "clients:entity:{id}"
///   TTL:  30 minutes
///   Scope: global (entity data is not user-specific at the query level)
///
/// Invalidation:
///   UpdateClientCommand and DeleteClientCommand must include
///   CacheKeys.Entity("clients", id) in their DirectInvalidationKeys.
/// </summary>
public record GetClientByIdQuery(Guid Id) : IRequest<AppResponse<CustomerResponse>>, ICachableRequest
{
    public string CacheGroup => "clients";
    public string Discriminator => Id.ToString();
    public string? CacheUserId => null;   // entity cache is shared across users
    public bool IsVersioned => false;  // invalidated directly by exact key
}

internal sealed class GetClientByIdQueryHandler(
    IUnitOfWork _unitOfWork,
    ILogger<GetClientByIdQueryHandler> _logger)
    : IRequestHandler<GetClientByIdQuery, AppResponse<CustomerResponse>>
{
    public async Task<AppResponse<CustomerResponse>> Handle(GetClientByIdQuery query, CancellationToken ct)
    {
        try
        {
            var customer = await _unitOfWork.CustomerRepository.FindByIdAsync(query.Id, ct).ConfigureAwait(false);

            if (customer is null)
                return AppResponse.Failure<CustomerResponse>($"Customer with ID {query.Id} was not found.");

            return AppResponse.Success(customer.ToCustomerResponse());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogGetCustomerByIdFailed(_logger, query.Id, ex);
            throw;
        }
    }
}
