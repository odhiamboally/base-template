using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Auth;

public record TwoFactorSetupInfo
{
    public string QrCodeUri { get; init; } = string.Empty;
    public string ManualEntryKey { get; init; } = string.Empty;
}
