namespace BT.Infrastructure.Configuration;

public sealed class StripePaymentSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string CheckoutSessionsEndpoint { get; set; } = "https://api.stripe.com/v1/checkout/sessions";
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}
