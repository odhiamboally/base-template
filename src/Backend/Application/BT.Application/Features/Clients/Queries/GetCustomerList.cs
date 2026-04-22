using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Extensions;
using BT.Application.Mappings;
using BT.Application.Utilities;
using BT.Domain.Contracts.Implementations.Specifications;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Specifications;
using BT.Domain.Entities;
using BT.Domain.Enums;
using BT.SharedKernel.Dtos.Client;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace BT.Application.Features.Clients.Queries;


// ════════════════════════════════════════════════════════════════════════════════
//  GET CLIENT LIST  (unfiltered, paginated)
// ════════════════════════════════════════════════════════════════════════════════
 
/// <summary>
/// Returns the full client list with cursor-based pagination, no search filters.
///
/// Cache strategy — VERSIONED list entry:
///   Key:  "clients:list:{userId}:{versionToken}:{discriminator}"
///   TTL:  30 minutes
///   Scope: per user — each RM sees only their relevant data
///
/// Invalidation: any mutation command bumps CacheKeys.GroupVersion("clients", userId),
/// which orphans every versioned entry for that user in O(1).
/// </summary>
public record GetCustomerListQuery(CustomerListRequest ClientListRequest, string UserId)
    : IRequest<AppResponse<PagedResponse<CustomerResponse, Guid>>>, ICachableRequest
{
    public string CacheGroup => "customers";
    public string Discriminator => CacheKeys.Discriminator(new CustomerListRequest(ClientListRequest.Cursor, ClientListRequest.PageSize));
    public string? CacheUserId => UserId;
    public bool IsVersioned => true;
}

internal sealed class GetCustomerListQueryHandler(IBankingUnitOfWork _bankingUow, ILogger<GetCustomerListQueryHandler> _logger) 
    : IRequestHandler<GetCustomerListQuery, AppResponse<PagedResponse<CustomerResponse, Guid>>>
{
    public async Task<AppResponse<PagedResponse<CustomerResponse, Guid>>> Handle(GetCustomerListQuery query, CancellationToken ct)
    {
        try
        {
            var req = query.ClientListRequest;

            var pageSize = Math.Clamp(req.PageSize, 1, 50);

            var totalCount = await _bankingUow.CustomerRepository.CountAsync(ct).ConfigureAwait(false);
            var customerEntities = await _bankingUow.CustomerRepository
                .FindAll()
                .Include(c => c.RelationshipManager)
                .Where(c => req.Cursor == null || req.Cursor == Guid.Empty || c.Id > req.Cursor)
                .OrderBy(c => c.Id)
                .Take(req.PageSize + 1)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var hasNextPage = customerEntities.Count > req.PageSize;

            if (hasNextPage)
                customerEntities.RemoveAt(customerEntities.Count - 1);

            var items = customerEntities.Select(c => c.ToCustomerResponse()).ToList();
            var nextCursor = hasNextPage ? items[^1].Id : (Guid?)null;

            bool isFirstPage = req.Cursor == null || req.Cursor == Guid.Empty;

            var pagedResult = new PagedResponse<CustomerResponse, Guid>(
                new Collection<CustomerResponse>(items),
                totalCount,
                1,
                pageSize,
                isFirstPage,
                nextCursor ?? Guid.Empty
            );

            return AppResponse.Success(pagedResult);
        }
        catch (Exception ex)
        {
            LogDefinitions.LogCustomerListFetchFailed(_logger, ex);
            throw;
        }
    }
}

