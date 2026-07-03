using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;

namespace BT.Application.Features.Shared.Payments.Contracts.Interfaces;

public interface IPaymentGateway
{
    Task<AppResponse<PaymentInitiationResponse>> InitiateAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default);

    Task<AppResponse<PaymentStatusResponse>> GetStatusAsync(
        string paymentReference,
        CancellationToken cancellationToken = default);
}
