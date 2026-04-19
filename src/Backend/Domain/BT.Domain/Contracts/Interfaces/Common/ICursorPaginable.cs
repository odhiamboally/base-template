using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Contracts.Interfaces.Common;

public interface ICursorPaginable
{
    Guid Id { get; }
    DateTimeOffset CreatedAt { get; }
}

