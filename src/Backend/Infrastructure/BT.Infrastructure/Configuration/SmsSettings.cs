namespace BT.Infrastructure.Configuration;

public class SmsSettings 
{
    public Settings GetSettings { get; set; } = new();

    public class Settings
    {
        public TwilioSettings Twilio { get; set; } = new();

        public bool EnableProviderFallback { get; set; } = true;


    }

    public class TwilioSettings
    {
        public string ProviderName { get; set; } = "Twilio";
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;

    }

    



}
