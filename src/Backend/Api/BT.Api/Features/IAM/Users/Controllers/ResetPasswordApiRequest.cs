using BT.SharedKernel.Features.IAM.Users.Dtos;

namespace BT.Api.Features.IAM.Users.Controllers;

public sealed record ResetPasswordApiRequest(
    string Email,
    string? NewPassword,
    string? Password,
    string? ConfirmPassword)
    : ResetPasswordRequest(Email, NewPassword, Password, ConfirmPassword);
