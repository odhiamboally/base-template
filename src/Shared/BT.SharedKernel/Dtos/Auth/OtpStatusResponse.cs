using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Auth;

public record OtpStatusResponse(
    bool IsConfigured,
    bool IsEnabled,
    string ProviderName,
    string DisplayName
);
