using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BT.Domain.Banking.Enums;

public enum DirectorRelationType
{
    [Description("Director")]
    Director,

    [Description("Shareholder")]
    Shareholder,

    [Description("Signatory")]
    Signatory,

    [Description("Beneficial Owner")]
    BeneficialOwner,

    [Description("Guarantor")]
    Guarantor
}

