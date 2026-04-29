using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Banking.Enums;
using BT.Domain.HR.Entities;
using BT.Domain.HR.Enums;
using BT.Domain.IAM.Enums;
using BT.Domain.Shared.Entities;
using BT.Domain.Shared.Enums;
using BT.Domain.Banking.Events;
using BT.Domain.HR.Events;
using BT.Domain.IAM.Events;
using BT.Domain.Shared.Events;
using BT.Domain.Exceptions;
using BT.Domain.Banking.ValueObjects;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BT.Domain.Banking.Entities;

/// <summary>
/// Client is the aggregate root for corporate client onboarding.
/// All state changes go through this entity — nothing is set directly
/// on owned entities from outside the aggregate.
/// </summary>
public class Customer : BaseEntity, ISoftDeletable, IHasDomainEvents
{
    // Header Info
    public string ClientNumber { get; private set; } = string.Empty;
    public string ClientName { get; private set; } = string.Empty;
    public CustomerType ClientType { get; private set; }
    public SegmentType SegmentType { get; private set; }
    public SubSegmentType SubSegmentType { get; private set; }
    public CustomerStatus Status { get; set; }

    // Management
    public DateTimeOffset OpenedOn { get; private set; }
    public Guid RelationshipManagerId { get; private set; }
    public Employee? RelationshipManager { get; private set; }

    // Navigation Properties for Tabs
    public CorporateDetail CorporateDetail { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public CommunicationPreference CommunicationPreference { get; private set; } = null!;

    private readonly List<Director> _directors = [];
    public IReadOnlyCollection<Director> Directors => _directors.AsReadOnly();

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    private Customer() { }

    /// <summary>
    /// Factory method — the only way to create a valid Client aggregate.
    /// Raises ClientCreatedEvent on success.
    /// </summary>
    public static Customer Create(
        string clientNumber,
        string clientName,
        CustomerType clientType,
        SegmentType segmentType,
        SubSegmentType subSegmentType,
        Guid rmId,
        DateTimeOffset openedOn,
        CorporateDetail corporateDetail,
        Address address,
        CommunicationPreference communicationPreference,
        string createdBy
    )
    {
        var client = new Customer
        {
            Id = Guid.CreateVersion7(), // The Id is created...
            ClientNumber = clientNumber,
            ClientName = clientName,
            ClientType = clientType,
            SegmentType = segmentType,
            SubSegmentType = subSegmentType,
            RelationshipManagerId = rmId,
            OpenedOn = openedOn,
            CorporateDetail = corporateDetail,
            Address = address,
            CommunicationPreference = communicationPreference,
            CreatedBy = createdBy
        };

        var domainEvent = new CustomerCreatedEvent(
            client.Id,
            client.ClientNumber,
            client.ClientName,
            client.Address.Email ?? string.Empty,
            client.ClientType
    );

        // Step 3: Raise the event on the newly created instance.
        client.RaiseDomainEvent(domainEvent);

        // Step 4: Return the fully constructed client with its pending domain event.
        return client;
    }

    // -------------------------------------------------------------------------
    // Behaviour methods — all state mutations go through here
    // -------------------------------------------------------------------------

    public void SetCorporateDetails(CorporateDetail corporateDetails)
    {
        ArgumentNullException.ThrowIfNull(corporateDetails);
        CorporateDetail = corporateDetails;
    }

    public void SetAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);
        Address = address;
    }

    public void SetCommunicationPreferences(CommunicationPreference preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        CommunicationPreference = preferences;
    }

    public void AssignRelationshipManager(Guid relationshipManagerId)
    {
        if (relationshipManagerId == Guid.Empty)
            throw new DomainException("A Relationship Manager must be assigned.");

        RelationshipManagerId = relationshipManagerId;
    }

    public void AddDirector(Director director)
    {
        ArgumentNullException.ThrowIfNull(director);

        var totalShares = _directors.Sum(d => d.SharePercentage) + director.SharePercentage;
        if (totalShares > 100)
        {
            throw new DomainException(
                $"Total share percentage cannot exceed 100%. Current total would be {totalShares}%.");
        }

        _directors.Add(director);
    }

    public void RemoveDirector(Guid directorId)
    {
        var director = _directors.FirstOrDefault(d => d.Id == directorId)
            ?? throw new DomainException("Director not found.");

        _directors.Remove(director);
    }

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void MarkAsDeleted(string deletedBy)
    {
        ArgumentNullException.ThrowIfNull(deletedBy);
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;

    }

}
