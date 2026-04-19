namespace BT.Infrastructure.Configuration;

public sealed class MessagingSettings
{
    public const string SectionName = "Messaging";

    public string Transport { get; set; } = "RabbitMq";
    public RabbitMqSettings RabbitMq { get; set; } = new();
    public AzureServiceBusSettings AzureServiceBus { get; set; } = new();

    public sealed class RabbitMqSettings
    {
        public string Host { get; set; } = "localhost";
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = "admin";
        public string Password { get; set; } = "guest";
    }

    public sealed class AzureServiceBusSettings
    {
        public string? ConnectionString { get; set; }
    }
}
