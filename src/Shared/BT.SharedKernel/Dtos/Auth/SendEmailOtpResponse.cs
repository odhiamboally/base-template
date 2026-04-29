using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Auth;


public sealed record SendEmailOtpResponse(
    string UserId,
    DateTimeOffset ExpiresAt,
    int CooldownSeconds
);