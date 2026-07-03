namespace BT.Infrastructure.Features.Shared.Payments.Contracts.Implementations;

internal enum PaymentProviderKind
{
    NoOp,
    Stripe,
    Mpesa,
    Invalid
}
