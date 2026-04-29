using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BT.Domain.Banking.Enums;

public enum CustomerType
{
    [Description("Individual")]
    Individual,

    [Description("Corporate")]
    Corporate,

    [Description("Institutional")]
    Institutional,

    [Description("SME")]
    SmallMediumEnterprise,

    [Description("Enterprise")]
    Enterprise
}
