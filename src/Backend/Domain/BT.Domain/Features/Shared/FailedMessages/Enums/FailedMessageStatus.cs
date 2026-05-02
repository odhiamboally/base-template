using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Features.Shared.FailedMessages.Enums;

public enum FailedMessageStatus
{
    Transient = 0,
    Permanent = 1,
    ManualRetry = 2
}
