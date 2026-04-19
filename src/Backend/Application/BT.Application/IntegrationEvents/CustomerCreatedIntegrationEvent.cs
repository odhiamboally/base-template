using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.IntegrationEvents;

public record CustomerCreatedIntegrationEvent(
    Guid ClientId,
    string ClientNumber,
    string ClientName,
    string Email,
    string ClientType) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}