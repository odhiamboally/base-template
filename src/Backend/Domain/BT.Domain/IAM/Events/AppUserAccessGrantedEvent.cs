using BT.Domain.Contracts.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.IAM.Events;


public sealed record AppUserAccessGrantedEvent(
    string Id,
    Guid? EmployeeId,
    Guid? CustomerId,
    string GrantedBy

) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
