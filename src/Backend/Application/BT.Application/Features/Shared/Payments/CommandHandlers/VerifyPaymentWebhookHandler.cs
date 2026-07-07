using BT.Application.Features.Shared.Payments.Contracts.Interfaces;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using MediatR;

namespace BT.Application.Features.Shared.Payments.CommandHandlers;

internal sealed class VerifyPaymentWebhookHandler(IPaymentWebhookVerifier paymentWebhookVerifier)
    : IRequestHandler<VerifyPaymentWebhookCommand, AppResponse<PaymentWebhookVerificationResponse>>
{
    public Task<AppResponse<PaymentWebhookVerificationResponse>> Handle(
        VerifyPaymentWebhookCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return paymentWebhookVerifier.VerifyAsync(
            request.Provider,
            request.Payload,
            request.SignatureHeader,
            cancellationToken);
    }
}
