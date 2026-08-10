using System;

namespace BT.SharedKernel.Features.ControlPlane.Auditing.Dtos;

public record EndImpersonationRequest(Guid ImpersonationRecordId);
