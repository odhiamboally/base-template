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
    public Task<AppResponse<IReadOnlyCollection<PaymentProviderCapabilityResponse>>> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return apiClient.SendAsync<IReadOnlyCollection<PaymentProviderCapabilityResponse>>(
            HttpMethod.Get,
            EndpointFormatter.Format(_apiSettings.Endpoints.Shared.Payments.Capabilities, _apiSettings.Version),
            unavailableMessage: "The payment service is unavailable. Please try again.",
            timeoutMessage: "The payment service timed out. Please try again.",
            cancellationToken: cancellationToken);
    }

    public Task<AppResponse<PaymentHistoryResponse>> GetHistoryAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return apiClient.SendAsync<PaymentHistoryResponse>(
            HttpMethod.Get,
            EndpointFormatter.Format(
                _apiSettings.Endpoints.Shared.Payments.History,
                _apiSettings.Version,
                new Dictionary<string, string>
                {
                    ["page"] = page.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["pageSize"] = pageSize.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }),
            unavailableMessage: "The payment service is unavailable. Please try again.",
            timeoutMessage: "The payment service timed out. Please try again.",
            cancellationToken: cancellationToken);
    }

    public Task<AppResponse<string>> RegisterMpesaC2BUrlsAsync(
        CancellationToken cancellationToken = default)
    {
        return apiClient.SendAsync<string>(
            HttpMethod.Post,
            EndpointFormatter.Format(_apiSettings.Endpoints.Shared.Payments.RegisterMpesaC2BUrls, _apiSettings.Version),
            unavailableMessage: "The payment service is unavailable. Please try again.",
            timeoutMessage: "The payment service timed out. Please try again.",
            cancellationToken: cancellationToken);
    }

    public Task<AppResponse<string>> SimulateMpesaC2BAsync(
        SimulateMpesaC2BRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return apiClient.SendAsync<string>(
            HttpMethod.Post,
            EndpointFormatter.Format(_apiSettings.Endpoints.Shared.Payments.SimulateMpesaC2B, _apiSettings.Version),
            request,
            unavailableMessage: "The payment service is unavailable. Please try again.",
            timeoutMessage: "The payment service timed out. Please try again.",
            cancellationToken: cancellationToken);
    }
}
