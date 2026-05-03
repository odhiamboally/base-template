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
    // HybridCache handles the "Get or Create" logic internally. 
    // Since your MediatR behavior handles the 'Create' part, we use it for simple retrieval.

    private static readonly ActivitySource CacheActivity = new("BT.Cache");
    private static readonly Meter CacheMeter = new("BT.Cache");
    private static readonly Counter<long> CacheHits = CacheMeter.CreateCounter<long>("cache_hits");
    private static readonly Counter<long> CacheMisses = CacheMeter.CreateCounter<long>("cache_misses");

    // Sentinel wrapper to distinguish null vs miss
    private sealed record CacheEnvelope<T>(bool HasValue, T? Value);

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        using var activity = CacheActivity.StartActivity("Cache GET");

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.operation", "GET");

        var start = Stopwatch.GetTimestamp();

        try
        {
            var envelope = await cache.GetOrCreateAsync(
                key,
                static _ => ValueTask.FromResult(new CacheEnvelope<T>(false, default)),
                cancellationToken: ct).ConfigureAwait(false);

            var elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

            var hit = envelope.HasValue;

            activity?.SetTag("cache.hit", hit);
            activity?.SetTag("cache.latency.ms", elapsedMs);

            if (hit)
            {
                CacheHits.Add(1);
                return envelope.Value;
            }

            CacheMisses.Add(1);
            return default;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            ServiceLogDefinitions.LogCacheOperationError(logger, "GET", key, ex);
            throw;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default)
    {
        using var activity = CacheActivity.StartActivity("Cache SET");

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.operation", "SET");
        activity?.SetTag("cache.expiration.seconds", expiration.TotalSeconds);

        var start = Stopwatch.GetTimestamp();

        try
        {
            var options = new HybridCacheEntryOptions
            {
                Expiration = expiration,
                LocalCacheExpiration = expiration
            };

            var envelope = new CacheEnvelope<T>(true, value);

            await cache.SetAsync(key, envelope, options, cancellationToken: ct)
                .ConfigureAwait(false);

            var elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

            activity?.SetTag("cache.latency.ms", elapsedMs);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            ServiceLogDefinitions.LogCacheOperationError(logger, "SET", key, ex);
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

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        var result = await GetAsync<object>(key, ct).ConfigureAwait(false);
        return result is not null;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
        => throw new NotSupportedException("Use version tokens instead.");
}
