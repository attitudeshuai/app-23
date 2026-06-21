using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class ContractRepository : Repository<Contract, int>, IContractRepository
{
    public ContractRepository(HabitContractDbContext context) : base(context)
    {
    }

    // 根据契约拥有者ID查询其所有契约
    public async Task<List<Contract>> GetByOwnerIdAsync(int ownerId)
    {
        return await _dbSet
            .Include(c => c.Partners)
            .Include(c => c.CheckIns)
            .Include(c => c.Violations)
            .Where(c => c.OwnerId == ownerId)
            .ToListAsync();
    }

    protected override IQueryable<Contract> ApplySearch(IQueryable<Contract> query, string searchTerm)
    {
        return query.Where(c => c.HabitName.Contains(searchTerm));
    }

    protected override IQueryable<Contract> ApplySorting(IQueryable<Contract> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "habitname" => descending ? query.OrderByDescending(c => c.HabitName) : query.OrderBy(c => c.HabitName),
            "status" => descending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            "startdate" => descending ? query.OrderByDescending(c => c.StartDate) : query.OrderBy(c => c.StartDate),
            "enddate" => descending ? query.OrderByDescending(c => c.EndDate) : query.OrderBy(c => c.EndDate),
            "createdat" => descending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };
    }
}
