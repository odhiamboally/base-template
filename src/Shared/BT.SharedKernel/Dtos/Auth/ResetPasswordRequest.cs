using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BT.SharedKernel.Dtos.Auth;
public abstract record ResetPasswordRequest(
    [Required, EmailAddress] string Email,
    [Required] string? NewPassword,
    [Required] string? Password,
    [Required] string? ConfirmPassword


    ) : IRequest<ResetPasswordRequest>;


