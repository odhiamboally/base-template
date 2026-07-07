namespace BT.SharedKernel.Features.Shared.Payments.Dtos;

public sealed record PaymentWebhookVerificationResponse(
    string Provider,
    string EventId,
    string EventType,
    string PaymentReference,
    string Status);
