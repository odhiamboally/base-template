namespace BT.SharedKernel.Features.Shared.Payments.Dtos;

public sealed record PaymentInitiationRequest(
    decimal Amount,
    string Currency,
    string Description,
    string CustomerReference,
    string CallbackUrl,
    string? PayerPhoneNumber = null,
    string? Provider = null);
