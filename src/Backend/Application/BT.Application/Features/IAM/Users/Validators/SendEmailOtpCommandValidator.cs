using BT.Application.Features.IAM.Users.Commands;
using BT.SharedKernel.Validation.Features.IAM.Users.Validators;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class SendEmailOtpCommandValidator : AbstractValidator<SendEmailOtpCommand>
{
    public SendEmailOtpCommandValidator()
    {
        RuleFor(command => command.Request)
            .NotNull()
            .SetValidator(new SendEmailOtpRequestValidator());
    }
}
