using BT.Domain.Contracts.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.IAM.Events;

public sealed record UserLogoutEvent(string AppUserId, string? SessionId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
