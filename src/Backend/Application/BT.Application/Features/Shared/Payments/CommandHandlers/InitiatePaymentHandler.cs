using BT.Application.Features.Shared.Payments.Contracts.Interfaces;
using BT.Domain.Features.Shared.Contracts;
using BT.Domain.Features.Shared.Payments.Entities;
using BT.Domain.Shared.ValueObjects;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;

using MediatR;

using System.Globalization;

namespace BT.Application.Features.Shared.Payments.CommandHandlers;

internal sealed class InitiatePaymentHandler(
    IPaymentGateway paymentGateway,
    ISharedUnitOfWork sharedUnitOfWork)
    : IRequestHandler<InitiatePaymentCommand, AppResponse<PaymentInitiationResponse>>
{
    public async Task<AppResponse<PaymentInitiationResponse>> Handle(
        InitiatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var req = request.Request;
        var provider = req.Provider ?? "Unknown";
        var providerPrefix = provider.Length >= 3 ? provider[..3].ToUpperInvariant() : provider.ToUpperInvariant();
        var paymentId = Guid.CreateVersion7();

        // Extract a human-readable, collision-resistant reference using the current date and the Guid's entropy
        // Format example: BT-STR-USD-260709-A1B2C3
        var timestamp = DateTimeOffset.UtcNow.ToString("yyMMdd", CultureInfo.InvariantCulture);
        var uniqueSuffix = paymentId.ToString("N")[^6..].ToUpperInvariant();
        var customerReference = $"BT-{providerPrefix}-{req.Currency}-{timestamp}-{uniqueSuffix}";

        var money = new Money(request.Request.Amount, request.Request.Currency);
        var paymentRecord = new PaymentRecord(
            id: paymentId,
            amount: money,
            description: request.Request.Description,
            customerReference: customerReference,
            provider: provider)
        {
            // CreatedBy is populated by the audit interceptor in SharedDBContext.SaveChangesAsync
            // via ICurrentActorProvider; defaults to "System" for unauthenticated flows.
            CreatedBy = string.Empty
        };

        var initiatePaymentResult = await paymentGateway
            .InitiateAsync(paymentRecord, request.Request, cancellationToken)
            .ConfigureAwait(false);

        if (initiatePaymentResult.IsSuccess)
        {
            await sharedUnitOfWork.PaymentRecordRepository.CreateAsync(paymentRecord, cancellationToken).ConfigureAwait(false);
            await sharedUnitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }

        return initiatePaymentResult;
    }
}
