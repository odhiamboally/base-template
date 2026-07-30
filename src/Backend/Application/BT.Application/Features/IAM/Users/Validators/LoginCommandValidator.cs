using BT.Application.Features.IAM.Users.Commands;
using BT.SharedKernel.Validation.Features.IAM.Users.Validators;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.LoginRequest)
            .NotNull()
            .SetValidator(new LoginRequestValidator());
    }
}
