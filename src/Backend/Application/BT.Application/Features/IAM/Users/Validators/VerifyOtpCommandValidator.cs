using BT.Application.Features.IAM.Users.Commands;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request.Code).NotEmpty().Matches("^\\d{6}$");
        RuleFor(command => command.Request.DeviceFingerprint).MaximumLength(256);
    }
}
