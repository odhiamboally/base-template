using BT.Application.Features.Shared.Payments.Contracts.Interfaces;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using MediatR;

namespace BT.Application.Features.Shared.Payments.CommandHandlers;

internal sealed class InitiatePaymentHandler(IPaymentGateway paymentGateway)
    : IRequestHandler<InitiatePaymentCommand, AppResponse<PaymentInitiationResponse>>
{
    public Task<AppResponse<PaymentInitiationResponse>> Handle(
        InitiatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return paymentGateway.InitiateAsync(request.Request, cancellationToken);
    }
}
