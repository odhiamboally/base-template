using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.IAM.Enums;
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
