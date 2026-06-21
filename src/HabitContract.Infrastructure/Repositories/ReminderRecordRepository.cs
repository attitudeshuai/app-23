using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class ReminderRecordRepository : IReminderRecordRepository
{
    private readonly HabitContractDbContext _context;
    private readonly DbSet<ReminderRecord> _dbSet;

    public ReminderRecordRepository(HabitContractDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ReminderRecord>();
    }

    public async Task<ReminderRecord?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Contract)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> HasSentTodayAsync(int contractId, int userId, DateTime date)
    {
        var dateStart = date.Date;
        var dateEnd = dateStart.AddDays(1);
        return await _dbSet.AnyAsync(r =>
            r.ContractId == contractId &&
            r.UserId == userId &&
            r.ReminderDate >= dateStart &&
            r.ReminderDate < dateEnd &&
            r.Status == ReminderStatus.Sent);
    }

    public async Task<IEnumerable<ReminderRecord>> GetPendingRemindersAsync()
    {
        return await _dbSet
            .Include(r => r.Contract)
            .Include(r => r.User)
            .Where(r => r.Status == ReminderStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReminderRecord>> GetByContractIdAsync(int contractId)
    {
        return await _dbSet
            .Include(r => r.User)
            .Where(r => r.ContractId == contractId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReminderRecord>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(r => r.Contract)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ReminderRecord> AddAsync(ReminderRecord record)
    {
        await _dbSet.AddAsync(record);
        return record;
    }

    public Task UpdateAsync(ReminderRecord record)
    {
        _dbSet.Update(record);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ReminderRecord record)
    {
        _dbSet.Remove(record);
        return Task.CompletedTask;
    }
}
