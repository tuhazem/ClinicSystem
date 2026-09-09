using ClinicSystem.Application.Common.Caching;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that intercepts commands implementing ICacheInvalidator
/// and evicts specified cache keys or tags upon successful response.
/// </summary>
public sealed class CacheInvalidationBehavior<TRequest, TResponse>(
    HybridCache hybridCache,
    ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, ICacheInvalidator
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (request.CacheKeysToInvalidate is { Count: > 0 } keys)
        {
            foreach (var key in keys)
            {
                logger.LogInformation("Invalidating cache key '{CacheKey}'", key);
                await hybridCache.RemoveAsync(key, cancellationToken);
            }
        }

        if (request.CacheTagsToInvalidate is { Count: > 0 } tags)
        {
            foreach (var tag in tags)
            {
                logger.LogInformation("Invalidating cache tag '{CacheTag}'", tag);
                await hybridCache.RemoveByTagAsync(tag, cancellationToken);
            }
        }

        return response;
    }
}
