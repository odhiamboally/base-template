using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Banking.Enums;
using BT.Domain.HR.Enums;
using BT.Domain.IAM.Enums;
using BT.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.IAM.Events;

public sealed record RequestPasswordResetEvent(
    Guid UserId,
    string FirstName,
    string PhoneNumber,
    string ValidationCode,
    string Email,
    string ResetToken,
    string ResetUrl,
    MfaChannel PreferredChannel

) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
