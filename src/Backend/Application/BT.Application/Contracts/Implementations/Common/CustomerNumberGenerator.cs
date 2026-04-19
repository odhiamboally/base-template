using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Interfaces.Common;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BT.Application.Contracts.Implementations.Common;

internal sealed class CustomerNumberGenerator(IUnitOfWork _unitOfWork) : ICustomerNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var prefix = "CLT";
        var totalCount = await _unitOfWork.CustomerRepository.CountAsync(ct).ConfigureAwait(false);
        var sequence = totalCount + 1;
        return $"{prefix}-{sequence:D5}"; // CLT-00001
    }
}

