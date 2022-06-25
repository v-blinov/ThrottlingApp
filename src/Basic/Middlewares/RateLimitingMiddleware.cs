﻿using System.Net;
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
    private readonly Settings _settings;
    private readonly IPAddress[]? _ipWhiteList;
    private readonly string[]? _clientWhiteList;

    private readonly object _locker =new();

    public RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache, IOptions<Settings> settings)
    {
        _next = next;
        _cache = cache;
        _settings = settings.Value;
        _ipWhiteList = settings.Value.IpWhitelist?.Select(IPAddress.Parse).ToArray();
        _clientWhiteList = settings.Value.ClientWhitelist?.ToArray();
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
        
        // ClientWhiteList
        var clientId = context.Request.Headers["ClientId"].ToString();
        if(!string.IsNullOrEmpty(clientId) && (_clientWhiteList?.Contains(clientId, StringComparer.InvariantCultureIgnoreCase) ?? false))
        {
            await _next(context);
            return;
        }

        int? timeWindow = null; 
        int? maxRequests = null; 
        
        // IpWhitelist
        var clientIp = context.Connection.RemoteIpAddress;
        if(clientIp is not null)
        {
            if(_ipWhiteList != null && _ipWhiteList.Contains(clientIp))
            {
                await _next(context);
                return;
            }

            var individualLimits = _settings.IndividualLimits?.FirstOrDefault(p => clientIp.Equals(IPAddress.Parse(p.Ip)));
            if(individualLimits is not null)
            {
                timeWindow = individualLimits.RateLimit.TimeWindow;
                maxRequests = individualLimits.RateLimit.MaxRequests;
            }
        }

        // Индивидуальные настройки для данного IP не заданы
        timeWindow ??= rateLimit.TimeWindow == default ? _settings.DefaultRateLimit.TimeWindow : rateLimit.TimeWindow;
        maxRequests ??= rateLimit.MaxRequests == default ? _settings.DefaultRateLimit.MaxRequests : rateLimit.MaxRequests;
        
        var key = GenerateClientKey(context);

        lock(_locker)
        {
            var clientStatistics = GetClientStatisticsByKey(key).GetAwaiter().GetResult();

            var dtLimitReset = clientStatistics?.LastSuccessfulResponseTime.AddSeconds(timeWindow.Value);
            if(clientStatistics is not null && DateTime.UtcNow < dtLimitReset && clientStatistics.NumberOfRequestsCompletedSuccessfully >= maxRequests.Value)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.Add("x-rate-limit-limit", timeWindow.Value.ToString());
                context.Response.Headers.Add("x-rate-limit-remaining", maxRequests.Value.ToString());
                context.Response.Headers.Add("x-rate-limit-reset", dtLimitReset.ToString());
                return;
            }

            UpdateClientStatisticsStorage(key, maxRequests.Value).GetAwaiter().GetResult();
        }

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
