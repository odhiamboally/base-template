using BT.Application.Features.Banking.Customers.Contracts.Interfaces;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BT.Application.Features.Banking.Customers.Contracts.Implementations;

internal sealed class CustomerNumberGenerator(IBankingUnitOfWork _bankingUnitOfWork) : ICustomerNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var prefix = "CUST";
        var totalCount = await _bankingUnitOfWork.CustomerRepository.CountAsync(ct).ConfigureAwait(false);
        var sequence = totalCount + 1;
        return $"{prefix}-{sequence:D5}"; // CUST-00001
    }
}

