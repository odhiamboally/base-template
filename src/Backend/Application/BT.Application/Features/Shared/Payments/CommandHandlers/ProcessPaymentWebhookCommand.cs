using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using MediatR;

namespace BT.Application.Features.Shared.Payments.CommandHandlers;

public sealed record ProcessPaymentWebhookCommand(
    string Provider,
    string Payload,
    string SignatureHeader) : IRequest<AppResponse<PaymentWebhookVerificationResponse>>;
