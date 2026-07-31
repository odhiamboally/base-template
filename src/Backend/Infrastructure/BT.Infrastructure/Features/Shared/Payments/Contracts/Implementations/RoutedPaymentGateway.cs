using BT.Domain.Features.Shared.Payments.Entities;
using BT.Application.Features.Shared.Payments.Contracts.Interfaces;
using BT.Infrastructure.Configuration;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using Microsoft.Extensions.Options;

namespace BT.Infrastructure.Features.Shared.Payments.Contracts.Implementations;

internal sealed class RoutedPaymentGateway(
    IOptions<PaymentSettings> options,
    NoOpPaymentGateway noOpPaymentGateway,
    StripePaymentGateway stripePaymentGateway,
    MpesaPaymentGateway mpesaPaymentGateway) : IPaymentGateway
{
    private readonly PaymentSettings _settings = options.Value;

    public Task<AppResponse<PaymentInitiationResponse>> InitiateAsync(
        PaymentRecord record,
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return ResolveGateway(request.Provider).InitiateAsync(record, request, cancellationToken);
    }

    public Task<AppResponse<PaymentStatusResponse>> GetStatusAsync(
        string paymentReference,
        string? provider = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentReference);

        return ResolveGateway(provider).GetStatusAsync(paymentReference, provider, cancellationToken);
    }

    private IPaymentGateway ResolveGateway(string? requestedProvider)
    {
        var effectiveProvider = string.IsNullOrWhiteSpace(requestedProvider)
            ? _settings.Provider
            : requestedProvider;

        return PaymentProviderParser.Parse(effectiveProvider) switch
        {
            PaymentProviderKind.NoOp => noOpPaymentGateway,
            PaymentProviderKind.Stripe => stripePaymentGateway,
            PaymentProviderKind.Mpesa => mpesaPaymentGateway,
            _ => new UnsupportedPaymentGateway(effectiveProvider)
        };
    }
}


