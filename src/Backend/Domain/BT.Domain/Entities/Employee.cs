using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Entities;

public class Employee : BaseEntity
{
    public string AppUserId { get; private set; } = string.Empty;
    public string EmployeeNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string IdNumber { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public Guid DepartmentId { get; private set; }
    public Guid ManagerId { get; private set; }
    

    private Employee() { } // EF Core

    public static Employee Create(
        string employeeNumber, 
        string email, 
        string fName, 
        string lName, 
        string idNumber,
        string phoneNumber,
        Guid departmentId, 
        Guid managerId, 
        string createdBy) => new()
    {
        Id = Guid.CreateVersion7(),
        EmployeeNumber = employeeNumber,
        Email = email,
        FirstName = fName,
        LastName = lName,
        IdNumber = idNumber,
        PhoneNumber = phoneNumber,
        DepartmentId = departmentId,
        ManagerId = managerId,
        CreatedBy = createdBy
    };
}
