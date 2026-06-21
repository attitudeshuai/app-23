using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class ContractViolationRepository : Repository<ContractViolation, int>, IContractViolationRepository
{
    public ContractViolationRepository(HabitContractDbContext context) : base(context)
    {
    }

    // 根据契约ID查询所有违约记录，按日期倒序
    public async Task<List<ContractViolation>> GetByContractIdAsync(int contractId)
    {
        return await _dbSet
            .Where(cv => cv.ContractId == contractId)
            .OrderByDescending(cv => cv.ViolationDate)
            .ToListAsync();
    }

    protected override IQueryable<ContractViolation> ApplySearch(IQueryable<ContractViolation> query, string searchTerm)
    {
        return query.Where(cv => cv.Reason.Contains(searchTerm));
    }

    protected override IQueryable<ContractViolation> ApplySorting(IQueryable<ContractViolation> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "violationdate" => descending ? query.OrderByDescending(cv => cv.ViolationDate) : query.OrderBy(cv => cv.ViolationDate),
            "isconfirmed" => descending ? query.OrderByDescending(cv => cv.IsConfirmed) : query.OrderBy(cv => cv.IsConfirmed),
            "createdat" => descending ? query.OrderByDescending(cv => cv.CreatedAt) : query.OrderBy(cv => cv.CreatedAt),
            _ => query.OrderByDescending(cv => cv.ViolationDate)
        };
    }
}
