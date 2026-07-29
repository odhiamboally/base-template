using System;

namespace BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;

public sealed record AddTenantModuleRequest
{
    public required string ModuleKey { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}
