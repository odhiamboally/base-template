using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Auth;


public sealed record VerifyEmailOtpResponse(
    string Token,
    string RefreshToken,
    string UserId,
    bool Success,
    DateTimeOffset ExpiresAt,
    AppUserResponse User,
    List<UserClaimsResponse> Claims
);