using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Banking.Enums;
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
