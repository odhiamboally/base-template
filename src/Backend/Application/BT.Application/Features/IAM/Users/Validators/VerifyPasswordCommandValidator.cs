using BT.Application.Features.IAM.Users.Commands;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class VerifyPasswordCommandValidator : AbstractValidator<VerifyPasswordCommand>
{
    public VerifyPasswordCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.Request.EmployeeNumber).MaximumLength(50);
        RuleFor(command => command.Request.Password).NotEmpty().MaximumLength(256);
    }
}
