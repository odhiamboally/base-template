using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BT.Domain.Banking.Enums;

public enum LineOfBusiness
{
    [Description("Agriculture")]
    Agriculture,

    [Description("Manufacturing")]
    Manufacturing,

    [Description("Technology")]
    Technology,

    [Description("Financial Services")]
    FinancialServices,

    [Description("Retail")]
    Retail,

    [Description("Services")]
    Services,

    [Description("Proprietary")]
    Proprietary,

    [Description("Trading")]
    Trading

}
