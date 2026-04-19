namespace BT.Infrastructure.Configuration;

public sealed class ObservabilitySettings
{
    public const string SectionName = "Observability";

    public bool Enabled { get; set; } = true;
    public string ServiceName { get; set; } = "LlanCore.BaseTemplate.API";
    public string ServiceNamespace { get; set; } = "LlanCore";
    public AzureMonitorSettings AzureMonitor { get; set; } = new();

    
}
