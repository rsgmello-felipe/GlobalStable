using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace GlobalStable.Infrastructure.Caching;
public class RedisCacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expirationTime ?? TimeSpan.FromMinutes(30),
        };

        var serializedData = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serializedData, options);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedData = await _cache.GetStringAsync(key);
        return cachedData is not null ? JsonSerializer.Deserialize<T>(cachedData) : default;
    }

    // 🔹 Remove um valor do cache
    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }
}
