using System;

namespace BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;

public class TenantResponse
{
    public Guid Id { get; set; }
    public required string Identifier { get; set; }
    public required string DisplayName { get; set; }
    public required string HostName { get; set; }
    public string? ContactEmail { get; set; }
    public int MaxUsers { get; set; }
    public required string SubscriptionTier { get; set; }
    public required string Status { get; set; }

    public Guid DeploymentStampId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
