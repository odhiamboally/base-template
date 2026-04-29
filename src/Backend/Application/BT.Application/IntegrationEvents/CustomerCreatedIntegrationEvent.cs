using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
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