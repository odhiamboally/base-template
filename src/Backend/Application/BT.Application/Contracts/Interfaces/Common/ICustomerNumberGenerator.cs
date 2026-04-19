using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Common;

internal interface ICustomerNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
