using System;

namespace BT.SharedKernel.Dtos.Employees;

public record CreateEmployeeRequest
{
    // Required fields
    public required string IdNumber { get; set; }
    public required string EmployeeNumber { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required DateOnly HireDate { get; init; }
    public required Guid DepartmentId { get; init; }
    public required Guid PositionId { get; init; }
    public required string Password { get; init; }

    // Optional fields
    public DateOnly? DateOfBirth { get; init; }
    public Guid? ManagerId { get; init; }
    
}
