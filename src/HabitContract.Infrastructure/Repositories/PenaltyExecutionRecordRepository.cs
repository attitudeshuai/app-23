using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class PenaltyExecutionRecordRepository : Repository<PenaltyExecutionRecord, int>, IPenaltyExecutionRecordRepository
{
    public PenaltyExecutionRecordRepository(HabitContractDbContext context) : base(context)
    {
    }

    public async Task<List<PenaltyExecutionRecord>> GetByContractIdAsync(int contractId)
    {
        return await _dbSet
            .Where(per => per.ContractId == contractId)
            .OrderByDescending(per => per.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PenaltyExecutionRecord>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(per => per.UserId == userId)
            .OrderByDescending(per => per.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PenaltyExecutionRecord>> GetByContractIdAndUserIdAsync(int contractId, int userId)
    {
        return await _dbSet
            .Where(per => per.ContractId == contractId && per.UserId == userId)
            .OrderByDescending(per => per.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PenaltyExecutionRecord>> GetByViolationIdAsync(int violationId)
    {
        return await _dbSet
            .Where(per => per.ContractViolationId == violationId)
            .OrderByDescending(per => per.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PenaltyExecutionRecord>> GetTrendByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(per => per.CreatedAt >= startDate && per.CreatedAt <= endDate)
            .OrderBy(per => per.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PenaltyExecutionRecord>> GetByStatusAsync(PenaltyExecutionStatus status)
    {
        return await _dbSet
            .Where(per => per.Status == status)
            .OrderByDescending(per => per.CreatedAt)
            .ToListAsync();
    }

    protected override IQueryable<PenaltyExecutionRecord> ApplySearch(IQueryable<PenaltyExecutionRecord> query, string searchTerm)
    {
        return query.Where(per =>
            per.CalculatedContent.Contains(searchTerm) ||
            (per.Details != null && per.Details.Contains(searchTerm)) ||
            (per.WaivedReason != null && per.WaivedReason.Contains(searchTerm)));
    }

    protected override IQueryable<PenaltyExecutionRecord> ApplySorting(IQueryable<PenaltyExecutionRecord> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "contractid" => descending ? query.OrderByDescending(per => per.ContractId) : query.OrderBy(per => per.ContractId),
            "userid" => descending ? query.OrderByDescending(per => per.UserId) : query.OrderBy(per => per.UserId),
            "penaltytype" => descending ? query.OrderByDescending(per => per.PenaltyType) : query.OrderBy(per => per.PenaltyType),
            "severity" => descending ? query.OrderByDescending(per => per.Severity) : query.OrderBy(per => per.Severity),
            "status" => descending ? query.OrderByDescending(per => per.Status) : query.OrderBy(per => per.Status),
            "financialamount" => descending ? query.OrderByDescending(per => per.FinancialAmount) : query.OrderBy(per => per.FinancialAmount),
            "executiondeadline" => descending ? query.OrderByDescending(per => per.ExecutionDeadline) : query.OrderBy(per => per.ExecutionDeadline),
            "createdat" => descending ? query.OrderByDescending(per => per.CreatedAt) : query.OrderBy(per => per.CreatedAt),
            _ => query.OrderByDescending(per => per.CreatedAt)
        };
    }
}
