using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BT.Application.Contracts.Implementations.Common;

internal sealed class CustomerNumberGenerator(IBankingUnitOfWork _bankingUnitOfWork) : ICustomerNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var prefix = "CLT";
        var totalCount = await _bankingUnitOfWork.CustomerRepository.CountAsync(ct).ConfigureAwait(false);
        var sequence = totalCount + 1;
        return $"{prefix}-{sequence:D5}"; // CLT-00001
    }
}

