using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BT.Application.Behaviours;

// <summary>
/// MediatR pipeline behavior that provides transparent read-through caching
/// for any query implementing <see cref="ICachableRequest"/>.
///
/// Registration order matters — register this AFTER validation behaviors
/// so invalid requests are rejected before a cache lookup.
///
/// Key assembly:
///   Non-versioned  →  "{group}:entity:{discriminator}"
///   Versioned      →  "{group}:list:{scope}:{versionToken}:{discriminator}"
///
/// Version token lifecycle:
///   Created lazily on the first cache miss; stored with a long TTL (24 h).
///   Bumped (replaced) by <see cref="CacheInvalidationBehavior{TRequest,TResponse}"/>
///   whenever a mutation command succeeds, which orphans all versioned entries
///   in the group without any key scanning.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse>(ICacheService cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICachableRequest
{
    // Version tokens live longer than the entries they version.
    // If a version token expires, the next request simply creates a new one —
    // effectively a full cache miss for the group, which is safe.
    private static readonly TimeSpan VersionTtl = TimeSpan.FromDays(1);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        // ── 1. Bypass check ────────────────────────────────────────────────────
        if (request.BypassCache)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        // ── 2. Key assembly ────────────────────────────────────────────────────
        string cacheKey;

        if (request.IsVersioned)
        {
            var scope = (request.CacheUserId ?? "global").ToLowerInvariant();
            var sentinelKey = CacheKeys.GroupVersion(request.CacheGroup, request.CacheUserId);
            var versionToken = await ResolveOrCreateVersionAsync(sentinelKey, cancellationToken).ConfigureAwait(false);

            cacheKey = CacheKeys.VersionedList(
                request.CacheGroup,
                scope,
                versionToken,
                request.Discriminator);
        }
        else
        {
            cacheKey = CacheKeys.Entity(request.CacheGroup, request.Discriminator);
        }

        // ── 3. Cache hit ────────────────────────────────────────────────────────
        var cached = await cache.GetAsync<TResponse>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            CacheLogDefinitions.LogCacheHit(logger, cacheKey);
            return cached;
        }

        // ── 4. Cache miss — execute handler ────────────────────────────────────
        CacheLogDefinitions.LogCacheMiss(logger, cacheKey);
        var response = await next(cancellationToken).ConfigureAwait(false);

        // ── 5. Store result ────────────────────────────────────────────────────
        if (response is not null)
        {
            var ttl = request.Expiration ?? TimeSpan.FromMinutes(30);
            await cache.SetAsync(cacheKey, response, ttl, cancellationToken).ConfigureAwait(false);
            CacheLogDefinitions.LogCacheSet(logger, cacheKey, ttl);
        }

        return response;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private async Task<string> ResolveOrCreateVersionAsync(string sentinelKey, CancellationToken ct)
    {
        var version = await cache.GetAsync<string>(sentinelKey, ct).ConfigureAwait(false);

        if (version is not null)
            return version;

        version = GenerateVersion();
        await cache.SetAsync(sentinelKey, version, VersionTtl, ct).ConfigureAwait(false);
        CacheLogDefinitions.LogCacheSet(logger, sentinelKey, VersionTtl);
        return version;
    }

    private static string GenerateVersion()
        => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
}