using System.ComponentModel.DataAnnotations;
namespace BT.SharedKernel.Features.Shared.OrgSettings.Dtos;

public record CreateOrgSettingRequest(
    [Required] string Key,
    [Required] string Value,
    string? Description);
