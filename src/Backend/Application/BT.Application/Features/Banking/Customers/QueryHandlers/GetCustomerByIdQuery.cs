using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.Banking.Customers.Mappings;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Features.IAM.Users.Mappings;
using BT.Application.Features.Shared.EmailTemplates.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.SharedKernel.Features.Banking.Customers.Dtos;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.Banking.Customers.QueryHandlers;

/// <summary>
/// Fetches a single customer by ID.
///
/// Cache strategy — NON-VERSIONED entity entry:
///   Key:  "customers:entity:tenant:{tenantId}:{id}"  (tenant-scoped)
///         "customers:entity:{id}"                     (platform admins only)
///   TTL:  30 minutes
///   Scope: per-tenant — customer data must never cross tenant boundaries.
///
/// Invalidation:
///   UpdateCustomerCommand and DeleteCustomerCommand must include
///   CacheKeys.Entity("customers", id) in their DirectInvalidationKeys.
///   CacheInvalidationBehavior automatically also removes the tenant-scoped variant.
/// </summary>

public record GetCustomerByIdQuery(Guid Id) : IRequest<AppResponse<CustomerResponse>>, ICachableRequest
{
    public string CacheGroup => "customers";
    public string Discriminator => Id.ToString();
    public string? CacheUserId => null;
    public bool IsVersioned => false;
}

