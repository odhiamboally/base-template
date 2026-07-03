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
///   Key:  "customers:entity:{id}"
///   TTL:  30 minutes
///   Scope: global (entity data is not user-specific at the query level)
///
/// Invalidation:
///   UpdateCustomerCommand and DeleteCustomerCommand must include
///   CacheKeys.Entity("customers", id) in their DirectInvalidationKeys.
/// </summary>


internal sealed class GetCustomerByIdQueryHandler(
    IBankingUnitOfWork _bankingUnitOfWork,
    ILogger<GetCustomerByIdQueryHandler> _logger)
    : IRequestHandler<GetCustomerByIdQuery, AppResponse<CustomerResponse>>
{
    public async Task<AppResponse<CustomerResponse>> Handle(GetCustomerByIdQuery query, CancellationToken ct)
    {
        try
        {
            var customer = await _bankingUnitOfWork.CustomerRepository.FindByIdAsync(query.Id, ct).ConfigureAwait(false);

            if (customer is null)
                return AppResponses.Failure<CustomerResponse>($"Customer with ID {query.Id} was not found.");

            return AppResponses.Success(customer.ToCustomerResponse());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogGetCustomerByIdFailed(_logger, query.Id, ex);
            throw;
        }
    }
}
