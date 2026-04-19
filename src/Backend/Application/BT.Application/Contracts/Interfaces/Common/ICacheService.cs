using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Common;

/// Abstraction over the cache store. Async-first; all operations accept a CancellationToken.
///
/// Implementations:
///   - InMemoryCacheService  (IMemoryCache  — single node, no serialization overhead)
///   - DistributedCacheService (IDistributedCache — Redis, SQL, etc.)
///
/// The interface intentionally has no dependency on any infrastructure type
/// (no MemoryCacheEntryOptions, no IDistributedCache). Infrastructure specifics
/// belong in the implementation, not the contract.
/// </summary>
public interface ICacheService
{
    // ── Read ──────────────────────────────────────────────────────────────────

    /// <summary>Returns the cached value, or null/default if the key does not exist.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Returns true if the key exists in the cache.</summary>
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);

    // ── Write ─────────────────────────────────────────────────────────────────

    /// <summary>Stores a value with an absolute expiration.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default);

    // ── Invalidate ────────────────────────────────────────────────────────────

    /// <summary>Removes a single key.</summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Removes all keys that begin with the given prefix.
    /// Prefer version-token invalidation over this; pattern removal is O(n) and
    /// not supported atomically by all distributed stores.
    /// Only use when you genuinely need to purge a named subset of entries.
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
}