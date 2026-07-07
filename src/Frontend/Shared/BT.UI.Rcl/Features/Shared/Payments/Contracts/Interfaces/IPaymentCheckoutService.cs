using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;

namespace BT.UI.Rcl.Features.Shared.Payments.Contracts.Interfaces;

public interface IPaymentCheckoutService
{
    Task<AppResponse<PaymentInitiationResponse>> CreateCheckoutAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default);

    Task<AppResponse<PaymentStatusResponse>> GetStatusAsync(
        string provider,
        string paymentReference,
        CancellationToken cancellationToken = default);
}
