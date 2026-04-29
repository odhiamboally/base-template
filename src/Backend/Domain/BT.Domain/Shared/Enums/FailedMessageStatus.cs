using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Shared.Enums;

public enum FailedMessageStatus
{
    Transient = 0,
    Permanent = 1,
    ManualRetry = 2
}
