using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BT.Domain.Features.Banking.Customers.Enums;

public enum RelationshipType
{
    [Description("Director")]
    Director,

    [Description("Shareholder")]
    Shareholder,

    [Description("Signatory")]
    Signatory,

    [Description("Beneficial Owner")]
    BeneficialOwner
}
