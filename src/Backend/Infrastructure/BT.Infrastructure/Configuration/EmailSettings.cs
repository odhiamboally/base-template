namespace BT.Infrastructure.Configuration;
public class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ClientBaseUrl { get; set; } = string.Empty;
    public string TemplatePath { get; set; } = string.Empty;
    
}
