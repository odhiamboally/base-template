namespace BT.Domain.Shared.Contracts.Settings;

public interface ITenantSettingsProvider
{
    Task<string?> GetSettingAsync(string key, CancellationToken ct = default);
    Task<T?> GetSettingAsync<T>(string key, CancellationToken ct = default);
}
