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

    Task<AppResponse<IReadOnlyCollection<PaymentProviderCapabilityResponse>>> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default);

    Task<AppResponse<PaymentHistoryResponse>> GetHistoryAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<AppResponse<string>> RegisterMpesaC2BUrlsAsync(
        CancellationToken cancellationToken = default);

    Task<AppResponse<string>> SimulateMpesaC2BAsync(
        SimulateMpesaC2BRequest request,
        CancellationToken cancellationToken = default);
}
