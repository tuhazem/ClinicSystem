using System;
using System.Collections.Generic;

namespace ClinicSystem.Application.Common.Caching;

/// <summary>
/// Marker contract for MediatR queries that should automatically be cached via HybridCache.
/// </summary>
public interface ICachableQuery<out TResponse>
{
    /// <summary>
    /// Unique cache key for this specific query and parameter combination.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Custom expiration time. If null, default HybridCache duration is used.
    /// </summary>
    TimeSpan? Expiration => null;

    /// <summary>
    /// Tags associated with this cache entry for bulk invalidation (e.g., ["patients", "patient-123"]).
    /// </summary>
    IReadOnlyCollection<string>? Tags => null;
}
