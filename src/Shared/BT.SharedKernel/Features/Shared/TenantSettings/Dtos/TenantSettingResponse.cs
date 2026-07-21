using System;
using System.ComponentModel.DataAnnotations;

namespace BT.SharedKernel.Features.Shared.TenantSettings.Dtos;

public record TenantSettingResponse(
    [Required] Guid Id,
    [Required] string Key,
    [Required] string Value,
    string? Description,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy);
