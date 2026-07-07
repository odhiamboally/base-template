using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;

namespace BT.Application.Features.Shared.Payments.Contracts.Interfaces;

public interface IPaymentWebhookVerifier
{
    Task<AppResponse<PaymentWebhookVerificationResponse>> VerifyAsync(
        string provider,
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken = default);
}
