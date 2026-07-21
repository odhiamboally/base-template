using BT.Application.Features.Shared.Payments.Contracts.Interfaces;
using BT.Infrastructure.Configuration;
using BT.SharedKernel.Features.Shared.Payments.Dtos;

using Microsoft.Extensions.Options;

namespace BT.Infrastructure.Features.Shared.Payments.Contracts.Implementations;

internal sealed class PaymentProviderCatalog(IOptions<PaymentSettings> options) : IPaymentProviderCatalog
{
    public IReadOnlyCollection<PaymentProviderCapabilityResponse> GetCapabilities()
    {
        var settings = options.Value;
        var stripeConfigured = IsStripeConfigured(settings.Stripe);
        var mpesaConfigured = IsMpesaConfigured(settings.Mpesa);

        return
        [
            new(
                "Stripe",
                "Card checkout",
                true,
                stripeConfigured,
                GetStripeEnvironment(settings.Stripe),
                true,
                false,
                false),
            new(
                "Mpesa",
                "M-Pesa mobile money",
                true,
                mpesaConfigured,
                settings.Mpesa.Environment,
                false,
                true,
                true),
        ];
    }

    private static bool IsStripeConfigured(StripePaymentSettings settings) =>
        !string.IsNullOrWhiteSpace(settings.SecretKey) &&
        !string.IsNullOrWhiteSpace(settings.CheckoutSessionsEndpoint) &&
        !string.IsNullOrWhiteSpace(settings.WebhookSigningSecret) &&
        Uri.TryCreate(settings.SuccessUrl, UriKind.Absolute, out _) &&
        Uri.TryCreate(settings.CancelUrl, UriKind.Absolute, out _);

    private static bool IsMpesaConfigured(MpesaPaymentSettings settings) =>
        !string.IsNullOrWhiteSpace(settings.ConsumerKey) &&
        !string.IsNullOrWhiteSpace(settings.ConsumerSecret) &&
        !string.IsNullOrWhiteSpace(settings.ShortCode) &&
        !string.IsNullOrWhiteSpace(settings.PassKey) &&
        Uri.TryCreate(settings.CallbackUrlBase, UriKind.Absolute, out _);

    private static string GetStripeEnvironment(StripePaymentSettings settings) =>
        settings.SecretKey.StartsWith("sk_live_", StringComparison.OrdinalIgnoreCase) ? "Live" : "Test";
}
