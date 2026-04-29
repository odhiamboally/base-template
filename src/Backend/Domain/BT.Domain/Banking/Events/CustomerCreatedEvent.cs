using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Banking.Enums;
using BT.Domain.HR.Enums;
using BT.Domain.IAM.Enums;
using BT.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Banking.Events;

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
