using BT.Application.Features.Shared.Payments.Contracts.Interfaces;
using BT.Infrastructure.Configuration;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BT.Infrastructure.Features.Shared.Payments.Contracts.Implementations;

internal sealed class NoOpPaymentGateway(
    IOptions<PaymentSettings> options,
    IHostEnvironment environment) : IPaymentGateway
{
    private readonly PaymentSettings _settings = options.Value;

    public Task<AppResponse<PaymentInitiationResponse>> InitiateAsync(
        BT.Domain.Features.Shared.Payments.Entities.PaymentRecord record,
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (environment.IsProduction() && !_settings.AllowNoOpInProduction)
        {
            return Task.FromResult(AppResponses.Failure<PaymentInitiationResponse>(
                "Payment provider is not configured for this environment."));
        }

        var reference = $"noop-{Guid.NewGuid():N}";
        return Task.FromResult(AppResponses.Success("Payment provider is running in no-op mode.",
            new PaymentInitiationResponse("NoOp", reference, string.Empty, "Pending")));
    }

    public Task<AppResponse<PaymentStatusResponse>> GetStatusAsync(
        string paymentReference,
        string? provider = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentReference);

        if (environment.IsProduction() && !_settings.AllowNoOpInProduction)
        {
            return Task.FromResult(AppResponses.Failure<PaymentStatusResponse>(
                "Payment provider is not configured for this environment."));
        }

        return Task.FromResult(AppResponses.Success("Payment provider is running in no-op mode.",
            new PaymentStatusResponse("NoOp", paymentReference, "Pending", 0m, string.Empty)));
    }
}
