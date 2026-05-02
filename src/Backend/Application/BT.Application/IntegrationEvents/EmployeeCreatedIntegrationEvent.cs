using BT.Domain.Shared.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.IntegrationEvents;

public sealed record EmployeeCreatedIntegrationEvent(
    Guid EmployeeId,
    string EmployeeNumber,
    string EmployeeName,
    string EmployeeEmail,
    string EmployeeType) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}