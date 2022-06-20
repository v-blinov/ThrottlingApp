using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Basic.Extensions;

public static class DistributedCachingExtensions
{
    public static async Task SetCahceValueAsync<T>(this IDistributedCache distributedCache, string key, T value, CancellationToken token = default)
    {
        await distributedCache.SetAsync(key, value.ToByteArray(), token);
    }

    public static async Task<T?> GetCacheValueAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken token = default) where T : class
    {
        var result = await distributedCache.GetAsync(key, token);
        return result.FromByteArray<T>();
    }

    private static byte[]? ToByteArray(this object? objectToSerialize)
    {
        return objectToSerialize != null 
            ? Encoding.Default.GetBytes(JsonSerializer.Serialize(objectToSerialize))
            : null;
    }
    private static T? FromByteArray<T>(this byte[]? arrayToDeserialize) where T : class
    {
        return arrayToDeserialize != null
            ? JsonSerializer.Deserialize<T>(Encoding.Default.GetString(arrayToDeserialize))
            : default;
    }
}
