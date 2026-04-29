using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;

namespace BT.Domain.HR.Events;

public sealed record EmployeeCreatedEvent(
    Guid EmployeeId,
    string EmployeeNumber,
    string Email,
    string EmployeeName

) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
