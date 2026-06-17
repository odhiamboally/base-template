namespace BT.UI.Blazor.Configuration;

internal sealed class CacheSettings
{
    public const string SectionName = "CacheSettings";

    public string? ConnectionString { get; init; }
    public bool UseEntraId { get; init; }
    public string? PrincipalId { get; init; }
}
