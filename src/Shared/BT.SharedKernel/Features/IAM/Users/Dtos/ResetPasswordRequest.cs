using System.ComponentModel.DataAnnotations;

namespace BT.SharedKernel.Features.IAM.Users.Dtos;

public sealed record ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(512)]
    public string? NewPassword { get; init; }

    [Required]
    [StringLength(512)]
    public string? ConfirmPassword { get; init; }

    [StringLength(4096)]
    public string? ResetToken { get; init; }
}
