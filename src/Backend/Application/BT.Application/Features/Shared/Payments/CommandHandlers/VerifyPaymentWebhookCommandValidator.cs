using FluentValidation;

namespace BT.Application.Features.Shared.Payments.CommandHandlers;

public sealed class VerifyPaymentWebhookCommandValidator : AbstractValidator<VerifyPaymentWebhookCommand>
{
    public VerifyPaymentWebhookCommandValidator()
    {
        RuleFor(command => command.Provider)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.Payload)
            .NotEmpty();

        RuleFor(command => command.SignatureHeader)
            .NotEmpty();
    }
}
