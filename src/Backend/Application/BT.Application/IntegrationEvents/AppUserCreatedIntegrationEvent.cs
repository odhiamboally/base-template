using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.IntegrationEvents;

internal sealed record AppUserCreatedIntegrationEvent(
    string UserId,
    Guid TenantId,
    Guid? EmployeeId,
    string UserName,
    string FullName,
    string Email

) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

