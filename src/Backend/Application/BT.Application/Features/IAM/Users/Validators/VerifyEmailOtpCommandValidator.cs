using BT.Application.Features.IAM.Users.Commands;
using BT.Domain.Features.IAM.Users.Enums;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class VerifyEmailOtpCommandValidator : AbstractValidator<VerifyEmailOtpCommand>
{
    public VerifyEmailOtpCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request.Code).NotEmpty().Matches("^\\d{6}$");
        RuleFor(command => command.Request.Purpose)
            .NotEmpty()
            .Must(static purpose => Enum.TryParse<OtpPurpose>(purpose, ignoreCase: true, out _))
            .WithMessage("OTP purpose is not supported.");
        RuleFor(command => command.Request.DeviceFingerprint).MaximumLength(256);
    }
}
