using BT.Domain.Banking.Enums;
using BT.Domain.HR.Enums;
using BT.Domain.IAM.Enums;
using BT.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Mappings;

public static class OtpPurposeMapping
{
    public static OtpPurpose ToPurposeEnum(this string purpose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);

        return purpose switch
        {
            nameof(OtpPurpose.Login) => OtpPurpose.Login,
            nameof(OtpPurpose.EmailConfirmation) => OtpPurpose.EmailConfirmation,
            nameof(OtpPurpose.PasswordReset) => OtpPurpose.PasswordReset,

            _ => throw new ArgumentException($"Unknown OTP purpose: {purpose}")
        };
    }

    public static string ToPurposeString(this OtpPurpose purpose)
    {
        return purpose switch
        {
            OtpPurpose.Login => nameof(OtpPurpose.Login),
            OtpPurpose.EmailConfirmation => nameof(OtpPurpose.EmailConfirmation),
            OtpPurpose.PasswordReset => nameof(OtpPurpose.PasswordReset),

            _ => throw new ArgumentOutOfRangeException(nameof(purpose), purpose, "Undefined enum value")
        };
    }
}
