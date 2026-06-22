using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class MakeUpRequestRepository : Repository<MakeUpRequest, int>, IMakeUpRequestRepository
{
    public MakeUpRequestRepository(HabitContractDbContext context) : base(context)
    {
    }

    public async Task<List<MakeUpRequest>> GetByContractIdAsync(int contractId)
    {
        return await _dbSet
            .Include(m => m.User)
            .Include(m => m.Reviewer)
            .Where(m => m.ContractId == contractId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MakeUpRequest>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(m => m.Contract)
            .Include(m => m.Reviewer)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MakeUpRequest>> GetPendingRequestsAsync()
    {
        return await _dbSet
            .Include(m => m.Contract)
            .Include(m => m.User)
            .Where(m => m.Status == MakeUpRequestStatus.Pending)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<MakeUpRequest?> GetByDateAsync(int contractId, int userId, DateTime checkInDate)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.ContractId == contractId && m.UserId == userId && m.CheckInDate == checkInDate.Date);
    }

    protected override IQueryable<MakeUpRequest> ApplySearch(IQueryable<MakeUpRequest> query, string searchTerm)
    {
        return query.Where(m =>
            m.Reason.Contains(searchTerm) ||
            (m.RejectionReason != null && m.RejectionReason.Contains(searchTerm)));
    }

    protected override IQueryable<MakeUpRequest> ApplySorting(IQueryable<MakeUpRequest> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "checkindate" => descending ? query.OrderByDescending(m => m.CheckInDate) : query.OrderBy(m => m.CheckInDate),
            "createdat" => descending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt),
            "status" => descending ? query.OrderByDescending(m => m.Status) : query.OrderBy(m => m.Status),
            _ => query.OrderByDescending(m => m.CreatedAt)
        };
    }
}
