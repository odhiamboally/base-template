using System;

namespace BT.SharedKernel.Features.ControlPlane.Auditing.Dtos;

public record StartImpersonationRequest(
    Guid TargetTenantId,
    string Reason,
    int DurationHours = 1);
