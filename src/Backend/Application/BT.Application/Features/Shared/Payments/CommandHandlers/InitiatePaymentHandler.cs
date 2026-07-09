using BT.Application.Features.Shared.Payments.Contracts.Interfaces;
using BT.Domain.Features.Shared.Payments.Entities;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Shared.ValueObjects;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using MediatR;

namespace BT.Application.Features.Shared.Payments.CommandHandlers;

internal sealed class InitiatePaymentHandler(
    IPaymentGateway paymentGateway,
    IRepository<PaymentRecord> paymentRepository)
    : IRequestHandler<InitiatePaymentCommand, AppResponse<PaymentInitiationResponse>>
{
    public async Task<AppResponse<PaymentInitiationResponse>> Handle(
        InitiatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var provider = request.Request.Provider ?? "Unknown";
        var count = await paymentRepository.CountAsync(cancellationToken).ConfigureAwait(false);
        var providerPrefix = provider.Length >= 3 ? provider[..3].ToUpperInvariant() : provider.ToUpperInvariant();
        var customerReference = $"BT-{providerPrefix}-{request.Request.Currency}-{count + 1:D6}";
        
        var money = new Money(request.Request.Amount, request.Request.Currency);
        var paymentRecord = new PaymentRecord(
            id: Guid.CreateVersion7(),
            amount: money,
            description: request.Request.Description,
            customerReference: customerReference,
            provider: provider)
        {
            CreatedBy = "System"
        };

        await paymentRepository.CreateAsync(paymentRecord, cancellationToken).ConfigureAwait(false);

        // We pass the generated customer reference via a modified request or pass the PaymentRecord itself.
        // Let's pass the PaymentRecord along with the original request to the gateway.
        return await paymentGateway.InitiateAsync(paymentRecord, request.Request, cancellationToken).ConfigureAwait(false);
    }
}
