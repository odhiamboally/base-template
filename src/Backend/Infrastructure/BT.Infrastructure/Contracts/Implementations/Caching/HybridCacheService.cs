using BT.Application.Contracts.Interfaces.Common;
using BT.Infrastructure.Logging;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;

namespace BT.Infrastructure.Contracts.Implementations.Caching;

internal sealed class HybridCacheService(HybridCache cache, ILogger<HybridCacheService> logger) : ICacheService
{
    private static readonly ActivitySource CacheActivity = new("BT.Cache");

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken ct = default)
    {
        using var activity = CacheActivity.StartActivity("Cache GET_OR_CREATE");

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.operation", "GET_OR_CREATE");
        if (expiration.HasValue)
        {
            activity?.SetTag("cache.expiration.seconds", expiration.Value.TotalSeconds);
        }

        var start = Stopwatch.GetTimestamp();

        try
        {
            var options = expiration.HasValue
                ? new HybridCacheEntryOptions
                {
                    Expiration = expiration,
                    LocalCacheExpiration = expiration
                }
                : null;

            var result = await cache.GetOrCreateAsync(
                key,
                async token => await factory(token).ConfigureAwait(false),
                options,
                cancellationToken: ct).ConfigureAwait(false);

            var elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            activity?.SetTag("cache.latency.ms", elapsedMs);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            ServiceLogDefinitions.LogCacheOperationError(logger, "GET_OR_CREATE", key, ex);
            throw;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        using var activity = CacheActivity.StartActivity("Cache REMOVE");

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.operation", "REMOVE");

        try
        {
            await cache.RemoveAsync(key, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            ServiceLogDefinitions.LogCacheOperationError(logger, "REMOVE", key, ex);
            throw;
        }
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
        => throw new NotSupportedException("Use version tokens instead.");
}
