using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IPenaltyRuleRepository : IRepository<PenaltyRule, int>
{
    Task<List<PenaltyRule>> GetByContractIdAsync(int contractId);

    Task<PenaltyRule?> GetActiveByContractIdAsync(int contractId);
}
