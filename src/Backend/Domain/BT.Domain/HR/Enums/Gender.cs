using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BT.Domain.HR.Enums;

public enum Gender
{
    [Description("Male")]
    Male = 0,

    [Description("Female")]
    Female = 1,

    [Description("Other")]
    Other = 2
}

