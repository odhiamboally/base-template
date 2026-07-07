using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using BT.UI.Blazor.Configuration;
using BT.UI.Blazor.Features.Shared.BackendApi;
using BT.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using BT.UI.Rcl.Features.Shared.Payments.Contracts.Interfaces;
using Microsoft.Extensions.Options;

namespace BT.UI.Blazor.Features.Shared.Payments.Contracts.Implementations;

internal sealed class PaymentCheckoutService(
    IBackendApiClient apiClient,
    IOptions<BackendApiSettings> apiSettings) : IPaymentCheckoutService
{
    private readonly BackendApiSettings _apiSettings = apiSettings.Value;

    public Task<AppResponse<PaymentInitiationResponse>> CreateCheckoutAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return apiClient.SendAsync<PaymentInitiationResponse>(
            HttpMethod.Post,
            EndpointFormatter.Format(_apiSettings.Endpoints.Shared.Payments.Checkout, _apiSettings.Version),
            request,
            unavailableMessage: "The payment service is unavailable. Please try again.",
            timeoutMessage: "The payment service timed out. Please try again.",
            cancellationToken: cancellationToken);
    }

    public Task<AppResponse<PaymentStatusResponse>> GetStatusAsync(
        string provider,
        string paymentReference,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentReference);

        return apiClient.SendAsync<PaymentStatusResponse>(
            HttpMethod.Get,
            EndpointFormatter.Format(
                _apiSettings.Endpoints.Shared.Payments.Status,
                _apiSettings.Version,
                new Dictionary<string, string>
                {
                    ["provider"] = provider,
                    ["paymentReference"] = paymentReference
                }),
            unavailableMessage: "The payment service is unavailable. Please try again.",
            timeoutMessage: "The payment service timed out. Please try again.",
            cancellationToken: cancellationToken);
    }
}
