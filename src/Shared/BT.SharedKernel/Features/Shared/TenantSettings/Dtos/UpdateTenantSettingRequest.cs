using System;
using System.ComponentModel.DataAnnotations;

namespace BT.SharedKernel.Features.Shared.TenantSettings.Dtos;

public record UpdateTenantSettingRequest(
    [Required] Guid Id,
    [Required] string Key,
    [Required] string Value,
    string? Description);
