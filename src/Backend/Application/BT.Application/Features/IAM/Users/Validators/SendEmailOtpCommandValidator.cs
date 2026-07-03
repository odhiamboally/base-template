using BT.Application.Features.IAM.Users.Commands;
using BT.Domain.Features.IAM.Users.Enums;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class SendEmailOtpCommandValidator : AbstractValidator<SendEmailOtpCommand>
{
    public SendEmailOtpCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request.Purpose)
            .NotEmpty()
            .Must(BeKnownOtpPurpose)
            .WithMessage("OTP purpose is not supported.");
    }

    private static bool BeKnownOtpPurpose(string purpose)
        => Enum.TryParse<OtpPurpose>(purpose, ignoreCase: true, out _);
}
