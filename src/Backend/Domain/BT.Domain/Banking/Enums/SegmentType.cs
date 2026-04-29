using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BT.Domain.Banking.Enums;

public enum SegmentType
{
    [Description("Public Limited Company")]
    PublicLimitedCompany,

    [Description("Private Limited Company")]
    PrivateLimitedCompany,

    [Description("Sole Proprietorship")]
    SoleProprietorship,

    [Description("Corporate")]
    Corporate,

    [Description("Retail")]
    Retail,

    [Description("Small Medium Enterprise")]
    SME,

    [Description("Small Medium Enterprise")]
    Individual





}
