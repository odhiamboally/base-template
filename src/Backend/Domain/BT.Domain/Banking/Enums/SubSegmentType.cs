using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BT.Domain.Banking.Enums;

public enum SubSegmentType
{
    [Description("Local")]
    Local,

    [Description("Multinational")]
    Multinational,

    [Description("Government Owned")]
    GovernmentOwned,

    [Description("Partnership")]
    Partnership,

    [Description("PrivateLimitedCompany")]
    PrivateLimitedCompany,

    [Description("PublicLimitedCompany")]
    PublicLimitedCompany,

    [Description("Sole Proprietorship")]
    SoleProprietorship,

    [Description("Non Governmental Organisation")]
    NGO,

    [Description("Non Governmental Organisation")]
    Individual
}
