using BT.Application.Features.Shared.Payments.Contracts.Interfaces;
using BT.Infrastructure.Configuration;
using BT.Infrastructure.Features.Shared.Payments.Contracts.Implementations.Mpesa;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BT.Infrastructure.Features.Shared.Payments.Contracts.Implementations;

internal sealed class MpesaPaymentGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentSettings> options,
    ILogger<MpesaPaymentGateway> logger) : IPaymentGateway
{
    private readonly MpesaPaymentSettings _settings = options.Value.Mpesa;

    public async Task<AppResponse<PaymentInitiationResponse>> InitiateAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsConfiguredForInitiation())
        {
            return AppResponses.Failure<PaymentInitiationResponse>(
                "M-Pesa payment provider is not configured for this environment.");
        }

        if (string.IsNullOrWhiteSpace(request.PayerPhoneNumber))
        {
            return AppResponses.Failure<PaymentInitiationResponse>(
                "Payer phone number is required for M-Pesa payments.");
        }

        try
        {
            var accessToken = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return AppResponses.Failure<PaymentInitiationResponse>(
                    "M-Pesa authentication failed. Please try again.");
            }

            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var password = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ShortCode}{_settings.PassKey}{timestamp}"));

            var normalizedPhoneNumber = NormalizeMpesaPhone(request.PayerPhoneNumber);
            var providerRequest = new MpesaStkPushRequest(
                _settings.ShortCode,
                password,
                timestamp,
                "CustomerPayBillOnline",
                decimal.ToInt32(decimal.Round(request.Amount, 0, MidpointRounding.AwayFromZero)),
                normalizedPhoneNumber,
                _settings.ShortCode,
                normalizedPhoneNumber,
                string.IsNullOrWhiteSpace(_settings.CallbackUrl) ? request.CallbackUrl : _settings.CallbackUrl,
                string.IsNullOrWhiteSpace(request.CustomerReference)
                    ? _settings.AccountReference
                    : request.CustomerReference,
                request.Description);

            using var httpClient = httpClientFactory.CreateClient("Payments.Mpesa");
            httpClient.BaseAddress = new Uri(_settings.StkPushEndpoint);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await httpClient
                .PostAsJsonAsync(string.Empty, providerRequest, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                HttpClientLogDefinitions.LogExternalApiWarning(
                    logger,
                    "POST",
                    _settings.StkPushEndpoint,
                    (int)response.StatusCode);

                return AppResponses.Failure<PaymentInitiationResponse>(
                    "M-Pesa could not accept the payment initiation request.");
            }

            using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            var providerResponse = await JsonSerializer
                .DeserializeAsync<MpesaStkPushResponse>(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (providerResponse is null || string.IsNullOrWhiteSpace(providerResponse.CheckoutRequestId))
            {
                return AppResponses.Failure<PaymentInitiationResponse>(
                    "M-Pesa returned an incomplete payment initiation response.");
            }

            return AppResponses.Success(
                "M-Pesa STK push initiated.",
                new PaymentInitiationResponse(
                    "Mpesa",
                    providerResponse.CheckoutRequestId,
                    string.Empty,
                    providerResponse.ResponseDescription ?? "Pending"));
        }
        catch (Exception ex)
        {
            HttpClientLogDefinitions.LogExternalApiError(logger, "POST", _settings.StkPushEndpoint, ex);

            return AppResponses.Failure<PaymentInitiationResponse>(
                "M-Pesa payment provider is temporarily unavailable.");
        }
    }

    public async Task<AppResponse<PaymentStatusResponse>> GetStatusAsync(
        string paymentReference,
        string? provider = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentReference);

        if (!IsConfiguredForStatus())
        {
            return AppResponses.Failure<PaymentStatusResponse>(
                "M-Pesa payment status is not configured for this environment.");
        }

        try
        {
            var accessToken = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return AppResponses.Failure<PaymentStatusResponse>(
                    "M-Pesa authentication failed. Please try again.");
            }

            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var password = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ShortCode}{_settings.PassKey}{timestamp}"));
            var providerRequest = new MpesaStkQueryRequest(
                _settings.ShortCode,
                password,
                timestamp,
                paymentReference);

            using var httpClient = httpClientFactory.CreateClient("Payments.Mpesa");
            httpClient.BaseAddress = new Uri(_settings.StkQueryEndpoint);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await httpClient
                .PostAsJsonAsync(string.Empty, providerRequest, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                HttpClientLogDefinitions.LogExternalApiWarning(
                    logger,
                    "POST",
                    _settings.StkQueryEndpoint,
                    (int)response.StatusCode);

                return AppResponses.Failure<PaymentStatusResponse>(
                    "M-Pesa payment status could not be retrieved.");
            }

            using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            var providerResponse = await JsonSerializer
                .DeserializeAsync<MpesaStkQueryResponse>(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (providerResponse is null)
            {
                return AppResponses.Failure<PaymentStatusResponse>(
                    "M-Pesa returned an incomplete payment status response.");
            }

            return AppResponses.Success(
                "M-Pesa payment status retrieved.",
                new PaymentStatusResponse("Mpesa", paymentReference, providerResponse.NormalizedStatus, 0m, "KES"));
        }
        catch (Exception ex)
        {
            HttpClientLogDefinitions.LogExternalApiError(logger, "POST", _settings.StkQueryEndpoint, ex);

            return AppResponses.Failure<PaymentStatusResponse>(
                "M-Pesa payment provider is temporarily unavailable.");
        }
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient("Payments.Mpesa.Auth");
        httpClient.BaseAddress = new Uri(_settings.AuthEndpoint);
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_settings.ConsumerKey}:{_settings.ConsumerSecret}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await httpClient
            .GetAsync(string.Empty, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            HttpClientLogDefinitions.LogExternalApiWarning(
                logger,
                "GET",
                _settings.AuthEndpoint,
                (int)response.StatusCode);

            return null;
        }

        using var stream = await response.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        var providerResponse = await JsonSerializer
            .DeserializeAsync<MpesaAccessTokenResponse>(stream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return providerResponse?.AccessToken;
    }

    private bool IsConfiguredForInitiation() =>
        IsAuthConfigured() &&
        !string.IsNullOrWhiteSpace(_settings.ShortCode) &&
        !string.IsNullOrWhiteSpace(_settings.PassKey) &&
        Uri.TryCreate(_settings.StkPushEndpoint, UriKind.Absolute, out _);

    private bool IsConfiguredForStatus() =>
        IsConfiguredForInitiation() &&
        Uri.TryCreate(_settings.StkQueryEndpoint, UriKind.Absolute, out _);

    private bool IsAuthConfigured() =>
        !string.IsNullOrWhiteSpace(_settings.ConsumerKey) &&
        !string.IsNullOrWhiteSpace(_settings.ConsumerSecret) &&
        Uri.TryCreate(_settings.AuthEndpoint, UriKind.Absolute, out _);

    private static string NormalizeMpesaPhone(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return digits.StartsWith('0')
            ? $"254{digits[1..]}"
            : digits;
    }
}
