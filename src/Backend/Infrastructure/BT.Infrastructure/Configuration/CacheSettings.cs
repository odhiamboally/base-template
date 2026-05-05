namespace BT.Infrastructure.Configuration;

public sealed class CacheSettings
{
    public const string SectionName = "CacheSettings";

    public AzureCacheSettings? Azure { get; set; }
}
