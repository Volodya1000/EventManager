﻿using EventManager.Application.Interfaces.Services;
using EventManager.Infrastructure.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace EventManager.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly CacheOptions _cacheOptions;

    public CacheService(
        IDistributedCache cache,
        IOptions<CacheOptions> cacheOptions)
    {
        _cache = cache;
        _cacheOptions = cacheOptions.Value;
    }

    public async Task<byte[]> GetEventImageAsync(
        Guid eventId,
        string filename,
        CancellationToken cst = default)
    {
        var cacheKey = GenerateEventImageKey(eventId, filename);
        return await _cache.GetAsync(cacheKey, cst);
    }

    public async Task SetEventImageAsync(
        Guid eventId,
        string filename,
        byte[] imageBytes,
        CancellationToken cst = default)
    {
        var cacheKey = GenerateEventImageKey(eventId, filename);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheOptions.DefaultExpiration
        };
        await _cache.SetAsync(cacheKey, imageBytes, options, cst);
    }

    public async Task RemoveEventImageAsync(
        Guid eventId,
        string filename,
        CancellationToken cst = default)
    {
        var cacheKey = GenerateEventImageKey(eventId, filename);
        await _cache.RemoveAsync(cacheKey, cst);
    }

    private static string GenerateEventImageKey(Guid eventId, string filename)
    {
        return $"event_image:{eventId}:{filename}";
    }
}