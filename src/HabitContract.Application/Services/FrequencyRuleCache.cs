using Microsoft.Extensions.Caching.Memory;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;

namespace HabitContract.Application.Services;

public class FrequencyRuleCache : IFrequencyRuleCache
{
    private readonly IMemoryCache _cache;
    private readonly IFrequencyParser _parser;
    private const string CacheKeyPrefix = "frequency_rule_";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(24);
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(6);

    public FrequencyRuleCache(IMemoryCache cache, IFrequencyParser parser)
    {
        _cache = cache;
        _parser = parser;
    }

    public Task<FrequencyRule?> GetAsync(int contractId)
    {
        var key = GetCacheKey(contractId);
        var rule = _cache.Get<FrequencyRule>(key);
        return Task.FromResult(rule);
    }

    public Task SetAsync(int contractId, FrequencyRule rule, TimeSpan? expiration = null)
    {
        var key = GetCacheKey(contractId);
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
            SlidingExpiration = SlidingExpiration
        };
        _cache.Set(key, rule, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int contractId)
    {
        var key = GetCacheKey(contractId);
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public async Task<FrequencyRule> GetOrCreateAsync(int contractId, string frequencyString, TimeSpan? expiration = null)
    {
        var key = GetCacheKey(contractId);
        var rule = await _cache.GetOrCreateAsync(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration;
            entry.SlidingExpiration = SlidingExpiration;
            return Task.FromResult(_parser.Parse(frequencyString));
        });
        return rule ?? _parser.Parse(frequencyString);
    }

    private static string GetCacheKey(int contractId)
    {
        return $"{CacheKeyPrefix}{contractId}";
    }
}
