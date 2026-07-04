using BT.Application.Contracts.Interfaces.Common;
using BT.SharedKernel.Extensions;
using BT.Application.Features.Banking.Customers.Mappings;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Features.IAM.Users.Mappings;
using BT.Application.Features.Shared.EmailTemplates.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.Banking.Customers.Contracts.Specifications;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.Banking.Customers.Enums;
using BT.SharedKernel.Features.Banking.Customers.Dtos;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BT.Application.Features.Banking.Customers.QueryHandlers;


public record SearchCustomerListQuery(CustomerSearchRequest SearchRequest, string UserId)
    : IRequest<AppResponse<PagedResponse<CustomerResponse, Guid>>>, ICachableRequest
{

    public string CacheGroup => "customers";
    public string Discriminator => CacheKeys.Discriminator(new CustomerSearchRequest(
        SearchRequest.GlobalSearch,
        SearchRequest.Type,
        SearchRequest.SegmentType,
        SearchRequest.SubSegmentType,
        SearchRequest.IdentificationType,
        SearchRequest.LineOfBusiness,
        SearchRequest.Status,
        SearchRequest.RelationshipManagerId,
        SearchRequest.Cursor,
        SearchRequest.PageSize));

    public string? CacheUserId => null;
    public bool IsVersioned => true;
    public bool BypassCache => false;  // explicit; see XML doc above
}

