using System.Text.Json;

using BT.UI.Blazor.Features.IAM.Users.Contracts.Interfaces;
using BT.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace BT.UI.Blazor.Features.IAM.Users.Implementations;

internal sealed class DistributedTokenStore : IServerTokenStore
{
    private readonly IDistributedCache _cache;
    private readonly string _instanceKey;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(6);

    public DistributedTokenStore(IDistributedCache cache)
    {
        _cache = cache;
        // Per-scope unique key. Scoped DI in Blazor Server maps to the circuit scope.
        _instanceKey = $"tokens:{Guid.NewGuid():N}";
    }

    public async Task<bool> ClearAsync()
    {
        await _cache.RemoveAsync(_instanceKey).ConfigureAwait(false);
        return true;
    }

    public async Task<(string? AccessToken, string? RefreshToken, string? SessionId)> GetAsync()
    {
        var data = await _cache.GetStringAsync(_instanceKey).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(data))
        {
            return (null, null, null);
        }

        try
        {
            var dto = JsonSerializer.Deserialize<TokenDto>(data, JsonOptions);
            return (dto?.AccessToken, dto?.RefreshToken, dto?.SessionId);
        }
        catch (JsonException)
        {
            // Corrupted cache entry; treat as missing.
            return (null, null, null);
        }
    }

    public async Task<bool> SaveAsync(string? accessToken, string? refreshToken, string? sessionId)
    {
        var dto = new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            SessionId = sessionId
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = DefaultExpiration
        };

        await _cache.SetStringAsync(_instanceKey, json, options).ConfigureAwait(false);
        return true;
    }

    private sealed class TokenDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? SessionId { get; set; }
    }
}
