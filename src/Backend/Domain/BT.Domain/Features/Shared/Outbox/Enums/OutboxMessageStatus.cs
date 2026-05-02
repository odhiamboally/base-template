using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Features.Shared.Outbox.Enums;

public enum OutboxMessageStatus
{
    Pending,
    Processing,
    Processed,
    Failed
}
