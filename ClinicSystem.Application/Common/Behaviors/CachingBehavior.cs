using ClinicSystem.Application.Common.Caching;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Common.Behaviors;

/// <summary>
/// Cross-cutting MediatR pipeline behavior that automatically intercepts queries implementing ICachableQuery
/// and caches results using .NET 9 HybridCache with stampede protection.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse>(
    HybridCache hybridCache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, ICachableQuery<TResponse>
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;
        var expiration = request.Expiration ?? DefaultExpiration;
        var tags = request.Tags;

        logger.LogDebug("Checking cache for query '{QueryType}' with key '{CacheKey}'", typeof(TRequest).Name, cacheKey);

        var entryOptions = new HybridCacheEntryOptions
        {
            Expiration = expiration,
            LocalCacheExpiration = expiration
        };

        return await hybridCache.GetOrCreateAsync(
            cacheKey,
            async token =>
            {
                logger.LogInformation("Cache miss for key '{CacheKey}'. Executing database query...", cacheKey);
                return await next();
            },
            entryOptions,
            tags,
            cancellationToken);
    }
}
