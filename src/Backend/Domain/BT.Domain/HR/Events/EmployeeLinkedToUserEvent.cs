using BT.Domain.Contracts.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.HR.Events;

public sealed record EmployeeLinkedToUserEvent(string Id, Guid EmployeeId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

