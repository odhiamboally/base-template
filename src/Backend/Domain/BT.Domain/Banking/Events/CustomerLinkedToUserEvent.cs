using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Banking.Events;

public sealed record CustomerLinkedToUserEvent(string Id, Guid CustomerId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
