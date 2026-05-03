using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Features.IAM.Users.Dtos;

public record TwoFactorSetupInfo
{
    public string QrCodeUri { get; init; } = string.Empty;
    public string ManualEntryKey { get; init; } = string.Empty;
}
