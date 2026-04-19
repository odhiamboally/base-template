using BT.Domain.Contracts.Interfaces.Common;

namespace BT.Domain.Events;

public sealed record EmployeeCreatedEvent(
    Guid EmployeeId,
    string EmployeeNumber,
    string Email,
    string EmployeeName

) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}