using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class PenaltyRuleRepository : Repository<PenaltyRule, int>, IPenaltyRuleRepository
{
    public PenaltyRuleRepository(HabitContractDbContext context) : base(context)
    {
    }

    public async Task<List<PenaltyRule>> GetByContractIdAsync(int contractId)
    {
        return await _dbSet
            .Where(pr => pr.ContractId == contractId)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();
    }

    public async Task<PenaltyRule?> GetActiveByContractIdAsync(int contractId)
    {
        return await _dbSet
            .Where(pr => pr.ContractId == contractId && pr.IsActive)
            .OrderByDescending(pr => pr.CreatedAt)
            .FirstOrDefaultAsync();
    }

    protected override IQueryable<PenaltyRule> ApplySearch(IQueryable<PenaltyRule> query, string searchTerm)
    {
        return query.Where(pr =>
            (pr.Description != null && pr.Description.Contains(searchTerm)) ||
            pr.RuleExpression.Contains(searchTerm));
    }

    protected override IQueryable<PenaltyRule> ApplySorting(IQueryable<PenaltyRule> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "contractid" => descending ? query.OrderByDescending(pr => pr.ContractId) : query.OrderBy(pr => pr.ContractId),
            "penaltytype" => descending ? query.OrderByDescending(pr => pr.PenaltyType) : query.OrderBy(pr => pr.PenaltyType),
            "isactive" => descending ? query.OrderByDescending(pr => pr.IsActive) : query.OrderBy(pr => pr.IsActive),
            "createdat" => descending ? query.OrderByDescending(pr => pr.CreatedAt) : query.OrderBy(pr => pr.CreatedAt),
            _ => query.OrderByDescending(pr => pr.CreatedAt)
        };
    }
}
