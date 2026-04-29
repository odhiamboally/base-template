using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Banking.Enums;
using BT.Domain.HR.Enums;
using BT.Domain.IAM.Enums;
using BT.Domain.Shared.Enums;
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