using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.Domain.Features.IAM.Users.Enums;
using BT.SharedKernel.Validation.Validators.Common;
using FluentValidation;
using System;

namespace BT.SharedKernel.Validation.Features.IAM.Users.Validators;

public sealed class SendEmailOtpRequestValidator : Validator<SendEmailOtpRequest>
{
    public SendEmailOtpRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty().MaximumLength(450);
        RuleFor(request => request.Purpose)
            .NotEmpty()
            .Must(BeKnownOtpPurpose)
            .WithMessage("OTP purpose is not supported.");
    }

    private static bool BeKnownOtpPurpose(string purpose)
        => Enum.TryParse<OtpPurpose>(purpose, ignoreCase: true, out _);
}
