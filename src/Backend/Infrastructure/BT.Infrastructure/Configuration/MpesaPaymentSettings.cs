namespace BT.Infrastructure.Configuration;

public sealed class MpesaPaymentSettings
{
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string PassKey { get; set; } = string.Empty;
    public string AccountReference { get; set; } = "BaseTemplate";
    public string AuthEndpoint { get; set; } = string.Empty;
    public string StkPushEndpoint { get; set; } = string.Empty;
    public string StkQueryEndpoint { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
}
