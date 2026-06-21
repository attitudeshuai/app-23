using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class CheckInRepository : Repository<CheckIn, int>, ICheckInRepository
{
    public CheckInRepository(HabitContractDbContext context) : base(context)
    {
    }

    // 根据契约ID查询所有打卡记录，按日期倒序
    public async Task<List<CheckIn>> GetByContractIdAsync(int contractId)
    {
        return await _dbSet
            .Include(ci => ci.User)
            .Where(ci => ci.ContractId == contractId)
            .OrderByDescending(ci => ci.CheckInDate)
            .ToListAsync();
    }

    // 根据用户ID查询其所有打卡记录
    public async Task<List<CheckIn>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(ci => ci.Contract)
            .Where(ci => ci.UserId == userId)
            .OrderByDescending(ci => ci.CheckInDate)
            .ToListAsync();
    }

    protected override IQueryable<CheckIn> ApplySearch(IQueryable<CheckIn> query, string searchTerm)
    {
        return query.Where(ci =>
            ci.ProofText != null && ci.ProofText.Contains(searchTerm));
    }

    protected override IQueryable<CheckIn> ApplySorting(IQueryable<CheckIn> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "checkindate" => descending ? query.OrderByDescending(ci => ci.CheckInDate) : query.OrderBy(ci => ci.CheckInDate),
            "createdat" => descending ? query.OrderByDescending(ci => ci.CreatedAt) : query.OrderBy(ci => ci.CreatedAt),
            _ => query.OrderByDescending(ci => ci.CheckInDate)
        };
    }
}
