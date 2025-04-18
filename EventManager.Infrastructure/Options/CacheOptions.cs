using Microsoft.Extensions.Caching.Distributed;

namespace EventManager.Infrastructure.Options;

public class CacheOptions
{
    public TimeSpan DefaultExpiration { get; set; }
}