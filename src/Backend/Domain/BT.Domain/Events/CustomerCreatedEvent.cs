using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Events;

public sealed record CustomerCreatedEvent(
    Guid ClientId,
    string ClientNumber,
    string ClientName,
    string Email,
    CustomerType ClientType

) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}