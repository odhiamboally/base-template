using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Configurations;

public class ApiSettings
{
    public const string SectionName = "ApiSettings";

    public string BaseUrl { get; set; } = string.Empty;
}

