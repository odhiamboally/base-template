using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Shared.Enums;

public enum OutboxMessageStatus
{
    Pending,
    Processing,
    Processed,
    Failed
}
