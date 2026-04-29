using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.HR.Enums;
using BT.Domain.Banking.Events;
using BT.Domain.HR.Events;
using BT.Domain.IAM.Events;
using BT.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace BT.Domain.IAM.Entities;

public class AppUser : IdentityUser, ISoftDeletable, IHasDomainEvents
{
    public Guid TenantId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? CustomerId { get; set; }

    // Belongs here because it's the KYC anchor, not HR or CRM data
    public string NationalId { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;

    // Minimal personal data needed for auth UX only
    // Source of truth for these remains Employee/Customer
    // These are a CACHE of display data
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public Uri? ProfilePictureUrl { get; set; }


    public DateTimeOffset? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LastFailedLoginAt { get; set; }
    public bool RequirePasswordChange { get; set; }
    public DateTimeOffset? PasswordLastChanged { get; set; }

    public string? TotpSecret { get; set; }

    public bool IsActive { get; set; }


    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public string? ActivatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? DeactivatedAt { get; set; }
    public string? DeactivatedBy { get; set; }
    public string? DeactivationReason { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    [Timestamp]
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays",
        Justification = "Required by Entity Framework Core for optimistic concurrency control")]
    public byte[] RowVersion { get; set; } = [];



    [JsonIgnore]
    public virtual ICollection<RefreshToken> RefreshTokens { get; } = [];

    [JsonIgnore]
    public virtual ICollection<AppUserDevice> TrustedDevices { get; } = [];


    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();


    public static AppUser Create(
        Guid tenantId,
        Guid? employeeId,
        string userName,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string createdBy
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId.ToString());
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        return new AppUser
        {
            Id = Guid.CreateVersion7().ToString(), 
            TenantId = tenantId,
            EmployeeId = employeeId,
            UserName = userName,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            ConcurrencyStamp = Guid.NewGuid().ToString(), // Initialize ConcurrencyStamp for optimistic concurrency control
            SecurityStamp = Guid.NewGuid().ToString(), // Initialize other properties with default values if necessary
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    // Factory for Employee-linked user
    public static AppUser CreateForEmployee(
        Guid tenantId,
        Guid employeeId,
        string userName,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string nationalId,
        string createdBy)
    {
        // validation...
        return new AppUser
        {
            Id = Guid.CreateVersion7().ToString(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            CustomerId = null,
            UserName = userName,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            NationalId = nationalId,
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    // Factory for Customer-linked user
    public static AppUser CreateForCustomer(
        Guid tenantId,
        Guid customerId,
        string userName,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string nationalId,
        string createdBy)
    {
        // validation...
        return new AppUser
        {
            Id = Guid.CreateVersion7().ToString(),
            TenantId = tenantId,
            EmployeeId = null,
            CustomerId = customerId,
            // ...rest
        };
    }

    // Factory for system/admin users (no business relationship)
    public static AppUser CreateSystemUser(
        Guid tenantId,
        string userName,
        string email,
        string createdBy)
    {
        return new AppUser
        {
            Id = Guid.CreateVersion7().ToString(),
            TenantId = tenantId,
            EmployeeId = null,
            CustomerId = null,
            // ...rest
        };
    }

    // Linking behaviours — called when role changes post-creation
    public void LinkToEmployee(Guid employeeId)
    {
        if (EmployeeId.HasValue)
            throw new DomainException("User is already linked to an employee record.");

        EmployeeId = employeeId;
        RaiseDomainEvent(new EmployeeLinkedToUserEvent(Id, employeeId));
    }

    public void LinkToCustomer(Guid customerId)
    {
        if (CustomerId.HasValue)
            throw new DomainException("User is already linked to a customer record.");

        CustomerId = customerId;
        RaiseDomainEvent(new CustomerLinkedToUserEvent(Id, customerId));
    }

    public void RaiseAppUserCreatedEvent()
    {
        RaiseDomainEvent(new AppUserCreatedEvent(
            Id, 
            TenantId, 
            EmployeeId,
            UserName!, 
            $"{FirstName} {LastName}", 
            Email ?? string.Empty));
    }

    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LastFailedLoginAt = null;
    }

    public void MarkAsDeleted(string deletedBy)
    {
        ArgumentNullException.ThrowIfNull(deletedBy);
        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAt = DateTimeOffset.UtcNow;

    }

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);

    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void GrantAccess(string grantedBy, IEnumerable<string> defaultRoles)
    {
        if (IsActive)
            throw new DomainException("User account is already active.");

        IsActive = true;
        ActivatedAt = DateTimeOffset.UtcNow;
        ActivatedBy = grantedBy;
        RequirePasswordChange = true; // Force password set on first login

        RaiseDomainEvent(new AppUserAccessGrantedEvent(Id, EmployeeId, Guid.Empty, grantedBy));
    }

    public void RevokeAccess(string revokedBy, string reason)
    {
        if (!IsActive)
            throw new DomainException("User account is already inactive.");

        IsActive = false;
        DeactivatedAt = DateTimeOffset.UtcNow;
        DeactivatedBy = revokedBy;
        DeactivationReason = reason;

        RaiseDomainEvent(new AppUserAccessRevokedEvent(Id, EmployeeId, Guid.Empty, revokedBy, reason));
    }
}



