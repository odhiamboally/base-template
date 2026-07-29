using System;

namespace BT.SharedKernel.Features.ControlPlane.Stamps.Dtos;

public class DeploymentStampResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string TargetResourceGroup { get; set; }
    public required string IsolationTier { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
