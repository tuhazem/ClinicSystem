using System.Collections.Generic;

namespace ClinicSystem.Application.Common.Caching;

/// <summary>
/// Marker contract for commands that should invalidate specific cache keys or tags upon successful execution.
/// </summary>
public interface ICacheInvalidator
{
    /// <summary>
    /// Specific cache keys to remove.
    /// </summary>
    IReadOnlyCollection<string>? CacheKeysToInvalidate => null;

    /// <summary>
    /// Tags whose associated cache entries should be evicted.
    /// </summary>
    IReadOnlyCollection<string>? CacheTagsToInvalidate => null;
}
