using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class ReminderTemplateRepository : IReminderTemplateRepository
{
    private readonly HabitContractDbContext _context;
    private readonly DbSet<ReminderTemplate> _dbSet;

    public ReminderTemplateRepository(HabitContractDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ReminderTemplate>();
    }

    public async Task<IEnumerable<ReminderTemplate>> GetAllAsync()
    {
        return await _dbSet
            .OrderBy(t => t.Type)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<ReminderTemplate?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<ReminderTemplate?> GetByTypeAsync(ReminderTemplateType type)
    {
        return await _dbSet
            .Where(t => t.Type == type && t.IsActive)
            .OrderByDescending(t => t.IsDefault)
            .FirstOrDefaultAsync();
    }

    public async Task<ReminderTemplate?> GetDefaultAsync(ReminderTemplateType type)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Type == type && t.IsDefault && t.IsActive);
    }

    public async Task<ReminderTemplate> AddAsync(ReminderTemplate template)
    {
        await _dbSet.AddAsync(template);
        return template;
    }

    public Task UpdateAsync(ReminderTemplate template)
    {
        _dbSet.Update(template);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ReminderTemplate template)
    {
        _dbSet.Remove(template);
        return Task.CompletedTask;
    }
}
