using FluentValidation;

namespace BT.Application.Features.Shared.Payments.CommandHandlers;

public sealed class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(command => command.Request)
            .NotNull();

        RuleFor(command => command.Request.Amount)
            .GreaterThan(0);

        RuleFor(command => command.Request.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(command => command.Request.Description)
            .NotEmpty()
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.Request.CallbackUrl)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("CallbackUrl must be an absolute URL.");

        RuleFor(command => command.Request.Provider)
            .MaximumLength(50);
    }
}
