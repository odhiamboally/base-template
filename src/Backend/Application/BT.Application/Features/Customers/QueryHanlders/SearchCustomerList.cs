using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Extensions;
using BT.Application.Mappings;
using BT.Application.Utilities;
using BT.Domain.Contracts.Implementations.Specifications;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Enums;
using BT.SharedKernel.Dtos.Client;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BT.Application.Features.Customers.QueryHandlers;

public record SearchCustomerListQuery(CustomerSearchRequest SearchRequest, string UserId)
    : IRequest<AppResponse<PagedResponse<CustomerResponse, Guid>>>, ICachableRequest
{

    public string CacheGroup => "customers";
    public string Discriminator => CacheKeys.Discriminator(new CustomerSearchRequest(
        SearchRequest.GlobalSearch,
        SearchRequest.ClientType,
        SearchRequest.SegmentType,
        SearchRequest.SubSegmentType,
        SearchRequest.IdentificationType,
        SearchRequest.LineOfBusiness,
        SearchRequest.Status,
        SearchRequest.RelationshipManagerId,
        SearchRequest.Cursor,
        SearchRequest.PageSize));

    public string? CacheUserId => UserId;
    public bool IsVersioned => true;
    public bool BypassCache => false;  // explicit; see XML doc above
}

internal sealed class SearchCustomerListQueryHandler(IBankingUnitOfWork _bankingUnitOfWork, ILogger<SearchCustomerListQueryHandler> _logger)
    : IRequestHandler<SearchCustomerListQuery, AppResponse<PagedResponse<CustomerResponse, Guid>>>
{
    public async Task<AppResponse<PagedResponse<CustomerResponse, Guid>>> Handle(SearchCustomerListQuery query, CancellationToken ct)
    {
        try
        {
            var req = query.SearchRequest;

            var pageSize = Math.Clamp(req.PageSize, 1, 50);

            var clientType = req.ClientType?.ToEnum<CustomerType>();
            var segmentType = req.SegmentType?.ToEnum<SegmentType>();
            var subSegmentType = req.SubSegmentType?.ToEnum<SubSegmentType>();
            var identificationType = req.IdentificationType?.ToEnum<IdentificationType>();
            var lineOfBusiness = req.LineOfBusiness?.ToEnum<LineOfBusiness>();
            var status = req.Status?.ToEnum<CustomerStatus>();

            var spec = new CustomerSearchSpec(
                req.GlobalSearch,
                clientType,
                segmentType,
                subSegmentType,
                identificationType,
                lineOfBusiness,
                status,
                req.RelationshipManagerId,
                req.Cursor,
                pageSize

            );

            var totalCount = await _bankingUnitOfWork.CustomerRepository.CountAsync(spec, ct).ConfigureAwait(false);
            var customerEntities = await _bankingUnitOfWork.CustomerRepository.SearchAsync(spec, ct).ConfigureAwait(false);

            bool hasNextPage = customerEntities.Count > pageSize;
            if (hasNextPage)
                customerEntities.RemoveAt(customerEntities.Count - 1);

            var items = customerEntities.Select(x => x.ToCustomerResponse()).ToList();

            var nextCursor = hasNextPage ? items[^1].Id : (Guid?)null;

            // Display sort — purely cosmetic, does not affect pagination
            items = [.. items.OrderBy(x => x.ClientNumber, StringComparer.OrdinalIgnoreCase)];

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
