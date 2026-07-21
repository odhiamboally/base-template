using System.ComponentModel.DataAnnotations;
namespace BT.SharedKernel.Features.Shared.TenantSettings.Dtos;

public record CreateTenantSettingRequest(
    [Required] string Key,
    [Required] string Value,
    string? Description);
