using BT.Domain.Shared.Contracts.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using BT.Domain.Features.ControlPlane.Tenants.Enums;

namespace BT.Domain.Features.ControlPlane.Tenants.Entities;

public class Tenant : IAuditable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Identifier { get; set; } // URL-safe slug
    public required string DisplayName { get; set; }
    public required string HostName { get; set; }
    public string? ContactEmail { get; set; }
    public int MaxUsers { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; }
    public TenantStatus Status { get; set; }

    public Guid DeploymentStampId { get; set; }
    public DeploymentStamp? DeploymentStamp { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public required string CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    [Timestamp]
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", 
        Justification = "Required by Entity Framework Core for optimistic concurrency control")]
    public byte[] RowVersion { get; set; } = [];
}
