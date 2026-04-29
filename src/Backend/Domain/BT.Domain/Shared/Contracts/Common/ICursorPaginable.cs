using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Shared.Contracts.Common;

public interface ICursorPaginable
{
    Guid Id { get; }
    DateTimeOffset CreatedAt { get; }
}

