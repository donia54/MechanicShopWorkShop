using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly HybridCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        HybridCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICachedQuery cachedRequest)
        {
            return await next(cancellationToken);
        }

        var requestName = typeof(TRequest).Name;
        var cacheHit = true;

        var cachedResponse = await _cache.GetOrCreateAsync<TResponse>(
            cachedRequest.CacheKey,
            _ =>
            {
                cacheHit = false;
                return ValueTask.FromResult(default(TResponse)!);
            },
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableUnderlyingData
            },
            cancellationToken: cancellationToken);

        if (cacheHit)
        {
            _logger.LogInformation("Cache hit for {RequestName} with key {CacheKey}", requestName, cachedRequest.CacheKey);
            return cachedResponse;
        }

        var response = await next(cancellationToken);

        if (response is IResult result && result.IsSuccess)
        {
            _logger.LogInformation("Caching response for {RequestName} with key {CacheKey}", requestName, cachedRequest.CacheKey);

            await _cache.SetAsync(
                cachedRequest.CacheKey,
                response,
                new HybridCacheEntryOptions
                {
                    Expiration = cachedRequest.Expiration
                },
                cachedRequest.Tags,
                cancellationToken);
        }

        return response;
    }
}