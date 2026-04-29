using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Shared.Contracts.Common;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    string CreatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }

}
