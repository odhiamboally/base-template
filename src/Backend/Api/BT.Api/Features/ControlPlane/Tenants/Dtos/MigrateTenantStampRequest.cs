using System;

namespace BT.Api.Features.ControlPlane.Tenants.Dtos;

public record MigrateTenantStampRequest(
    Guid NewDeploymentStampId,
    string NewDatabaseConnectionString);
