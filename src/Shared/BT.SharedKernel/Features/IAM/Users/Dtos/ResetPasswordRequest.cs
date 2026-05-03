using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BT.SharedKernel.Features.IAM.Users.Dtos;
public abstract record ResetPasswordRequest(
    [Required, EmailAddress] string Email,
    [Required] string? NewPassword,
    [Required] string? Password,
    [Required] string? ConfirmPassword


    ) : IRequest<ResetPasswordRequest>;


