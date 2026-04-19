using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Events;

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