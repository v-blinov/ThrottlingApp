using System.Net;
using Basic.Attributes;
using Basic.Extensions;
using Basic.Models;
using Basic.Optional;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Basic.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly DefaultRateLimit _defaultRateLimit;

    public RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache, IOptions<Settings> options)
    {
        _next = next;
        _cache = cache;
        _defaultRateLimit = options.Value.DefaultRateLimit;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var rateLimit = endpoint?.Metadata.GetMetadata<LimitRequestsAttribute>();
        if (rateLimit is null)
        {
            await _next(context);
            return;
        }
        
        var timeWindow = rateLimit.TimeWindow == default ? _defaultRateLimit.TimeWindow : rateLimit.TimeWindow;
        var maxRequests = rateLimit.MaxRequests == default ? _defaultRateLimit.MaxRequests : rateLimit.MaxRequests;
        
        var key = GenerateClientKey(context);
        var clientStatistics = await GetClientStatisticsByKey(key);
        
        if (clientStatistics is not null 
         && DateTime.UtcNow < clientStatistics.LastSuccessfulResponseTime.AddSeconds(timeWindow)
         && clientStatistics.NumberOfRequestsCompletedSuccessfully == maxRequests)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            return;
        }
        
        await UpdateClientStatisticsStorage(key, maxRequests);
        await _next(context);
    }

    private static string GenerateClientKey(HttpContext context) 
        => $"{context.Request.Path}_{context.Connection.RemoteIpAddress}";
    
    private async Task<ClientStatistics?> GetClientStatisticsByKey(string key)
    {   
        return await _cache.GetCacheValueAsync<ClientStatistics>(key);
    }

    private async Task UpdateClientStatisticsStorage(string key, int maxRequests)
    {
        var clientStat = await _cache.GetCacheValueAsync<ClientStatistics>(key);

        if (clientStat != null)
        {
            clientStat.LastSuccessfulResponseTime = DateTime.UtcNow;

            if (clientStat.NumberOfRequestsCompletedSuccessfully == maxRequests)
                clientStat.NumberOfRequestsCompletedSuccessfully = 1;

            else
                clientStat.NumberOfRequestsCompletedSuccessfully++;

            await _cache.SetCahceValueAsync(key, clientStat);
        }
        else
        {
            var clientStatistics = new ClientStatistics
            {
                LastSuccessfulResponseTime = DateTime.UtcNow,
                NumberOfRequestsCompletedSuccessfully = 1
            };

            await _cache.SetCahceValueAsync(key, clientStatistics);
        }

    }
}
