using HabitContract.Domain.Common;

namespace HabitContract.Application.Interfaces;

public interface IFrequencyRuleCache
{
    Task<FrequencyRule?> GetAsync(int contractId);
    Task SetAsync(int contractId, FrequencyRule rule, TimeSpan? expiration = null);
    Task RemoveAsync(int contractId);
    Task<FrequencyRule> GetOrCreateAsync(int contractId, string frequencyString, TimeSpan? expiration = null);
}
