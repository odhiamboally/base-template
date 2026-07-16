using BT.Application.Features.IAM.Users.Commands;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.Request.Token).NotEmpty().MaximumLength(4096);
        RuleFor(command => command.Request.NewPassword ?? command.Request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.");
        RuleFor(command => command.Request.ConfirmPassword)
            .Equal(command => command.Request.NewPassword ?? command.Request.Password)
            .WithMessage("Password confirmation must match.");
    }
}
