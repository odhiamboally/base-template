using FluentValidation;

namespace BT.Application.Features.Shared.Payments.QueryHandlers;

public sealed class GetPaymentStatusQueryValidator : AbstractValidator<GetPaymentStatusQuery>
{
    public GetPaymentStatusQueryValidator()
    {
        RuleFor(query => query.Provider)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(query => query.PaymentReference)
            .NotEmpty()
            .MaximumLength(200);
    }
}
