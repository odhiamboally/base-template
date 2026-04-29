using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Common;

public record EmailTemplateResponse
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
}

