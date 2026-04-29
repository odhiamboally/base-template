using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BT.Domain.Banking.Enums;

public enum CustomerStatus
{
    [Description("Draft")]
    Draft,

    [Description("Active")]
    Active,

    [Description("Suspended")]
    Suspended,

    [Description("Closed")]
    Closed,

    [Description("Pending Approval")]
    PendingApproval
}
