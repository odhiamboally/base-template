using BT.Application.Features.IAM.Users.Commands;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.AccessToken).NotEmpty();
        RuleFor(command => command.Request.RefreshToken).NotEmpty();
    }
}
