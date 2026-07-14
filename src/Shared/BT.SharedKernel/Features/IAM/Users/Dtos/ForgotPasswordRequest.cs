using System.ComponentModel.DataAnnotations;

namespace BT.SharedKernel.Features.IAM.Users.Dtos;

public sealed record ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;
}
