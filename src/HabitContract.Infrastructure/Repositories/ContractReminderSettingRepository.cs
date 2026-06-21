using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class ContractReminderSettingRepository : IContractReminderSettingRepository
{
    private readonly HabitContractDbContext _context;
    private readonly DbSet<ContractReminderSetting> _dbSet;

    public ContractReminderSettingRepository(HabitContractDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ContractReminderSetting>();
    }

    public async Task<IEnumerable<ContractReminderSetting>> GetEnabledSettingsAsync()
    {
        return await _dbSet
            .Include(s => s.Contract)
            .Include(s => s.User)
            .Where(s => s.IsEnabled && s.Contract.Status == Domain.Enums.ContractStatus.Active)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContractReminderSetting>> GetByContractIdAsync(int contractId)
    {
        return await _dbSet
            .Include(s => s.User)
            .Where(s => s.ContractId == contractId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContractReminderSetting>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(s => s.Contract)
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.ReminderTime)
            .ToListAsync();
    }

    public async Task<ContractReminderSetting?> GetByContractAndUserAsync(int contractId, int userId)
    {
        return await _dbSet
            .Include(s => s.Contract)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.ContractId == contractId && s.UserId == userId);
    }

    public async Task<ContractReminderSetting> AddAsync(ContractReminderSetting setting)
    {
        await _dbSet.AddAsync(setting);
        return setting;
    }

    public Task UpdateAsync(ContractReminderSetting setting)
    {
        _dbSet.Update(setting);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ContractReminderSetting setting)
    {
        _dbSet.Remove(setting);
        return Task.CompletedTask;
    }
}
