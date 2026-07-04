using BT.Application.Features.IAM.Users.Commands;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.LoginRequest).NotNull();
        RuleFor(command => command.LoginRequest.UserName).NotEmpty().MaximumLength(256);
        RuleFor(command => command.LoginRequest.Password).NotEmpty().MaximumLength(256);
        RuleFor(command => command.LoginRequest.DeviceFingerprint).NotEmpty().MaximumLength(256);
        RuleFor(command => command.LoginRequest.ReturnUrl).MaximumLength(512);
    }
}
