using System.Text.Json;
using BT.Domain.Features.Shared.Contracts;
using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Shared.Contracts.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Contracts.Implementations.Settings;

public class CachedTenantSettingsProvider : ITenantSettingsProvider
{
    private readonly IDistributedCache _cache;
    private readonly ICurrentTenantProvider _tenantProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<CachedTenantSettingsProvider> _logger;

    public CachedTenantSettingsProvider(
        IDistributedCache cache,
        ICurrentTenantProvider tenantProvider,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IEncryptionService encryptionService,
        ILogger<CachedTenantSettingsProvider> logger)
    {
        _cache = cache;
        _tenantProvider = tenantProvider;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<string?> GetSettingAsync(string key, CancellationToken ct = default)
    {
        var tenantId = _tenantProvider.TenantId;
        if (tenantId == Guid.Empty)
        {
            // If no tenant context, fallback to global appsettings
            return _configuration[key];
        }

        var cacheKey = $"tenant:{tenantId}:settings:{key}";

        try
        {
            var cachedValue = await _cache.GetStringAsync(cacheKey, ct);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return cachedValue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read setting from cache.");
        }

        // Cache miss, read from DB
        string? plainTextValue = null;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<ISharedUnitOfWork>();
            var setting = await unitOfWork.TenantSettingRepository.FirstOrDefaultAsync(x => x.Key == key, ct);

            if (setting != null)
            {
                plainTextValue = _encryptionService.Decrypt(setting.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read tenant setting from database.");
        }

        // Hierarchy Fallback: if not in DB, fallback to appsettings
        if (string.IsNullOrEmpty(plainTextValue))
        {
            plainTextValue = _configuration[key];
        }

        // Cache it (even if null, cache empty string to prevent repeated DB hits)
        if (plainTextValue != null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };
                await _cache.SetStringAsync(cacheKey, plainTextValue, options, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write setting to cache.");
            }
        }

        return plainTextValue;
    }

    public async Task<T?> GetSettingAsync<T>(string key, CancellationToken ct = default)
    {
        var stringValue = await GetSettingAsync(key, ct);
        if (string.IsNullOrEmpty(stringValue))
        {
            return default;
        }

        try
        {
            // If T is a primitive type or string, try to convert directly
            if (typeof(T) == typeof(string))
            {
                return (T)(object)stringValue;
            }
            
            if (typeof(T).IsPrimitive || typeof(T) == typeof(decimal) || typeof(T) == typeof(Guid))
            {
                return (T)Convert.ChangeType(stringValue, typeof(T));
            }

            // Otherwise assume JSON
            return JsonSerializer.Deserialize<T>(stringValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize setting {Key} to type {Type}", key, typeof(T).Name);
            return default;
        }
    }
}
