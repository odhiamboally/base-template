using System.ComponentModel.DataAnnotations;
using BT.SharedKernel.Features.IAM.Users.Dtos;

namespace BT.UI.Blazor.Features.IAM.Users.Models;

internal sealed class SignInFormModel
{
    [Required(ErrorMessage = "Username or email is required.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

internal sealed record PasswordLoginRequest(
    string UserName,
    string Password,
    bool RememberMe,
    string? ReturnUrl,
    string DeviceFingerprint)
    : LoginRequest(UserName, Password, RememberMe, ReturnUrl, DeviceFingerprint);
