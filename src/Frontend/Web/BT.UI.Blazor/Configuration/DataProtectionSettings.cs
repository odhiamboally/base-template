namespace BT.UI.Blazor.Configuration;

internal sealed class DataProtectionSettings
{
    public const string SectionName = "DataProtection";

    public string ApplicationName { get; init; } = "BaseTemplate";
    public string? KeysPath { get; init; }
}
