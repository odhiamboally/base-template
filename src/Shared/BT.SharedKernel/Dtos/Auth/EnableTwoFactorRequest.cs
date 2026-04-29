using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Auth;

public record EnableTwoFactorRequest
{
    public string VerificationCode { get; init; } = string.Empty;
}
