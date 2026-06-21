using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class HabitTemplateRepository : Repository<HabitTemplate, int>, IHabitTemplateRepository
{
    public HabitTemplateRepository(HabitContractDbContext context) : base(context)
    {
    }

    public async Task<List<HabitTemplate>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<List<HabitTemplate>> GetPublishedTemplatesAsync()
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.Status == TemplateStatus.Published)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<List<HabitTemplate>> GetPublishedByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId && t.Status == TemplateStatus.Published)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<List<HabitTemplate>> GetRecommendedTemplatesAsync(int userId, int count)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.Status == TemplateStatus.Published)
            .OrderByDescending(t => t.UsageCount)
            .Take(count)
            .ToListAsync();
    }

    public override async Task<HabitTemplate?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public override async Task<IEnumerable<HabitTemplate>> GetAllAsync()
    {
        return await _dbSet
            .Include(t => t.Category)
            .ToListAsync();
    }

    protected override IQueryable<HabitTemplate> ApplySearch(IQueryable<HabitTemplate> query, string searchTerm)
    {
        return query.Where(t => t.Name.Contains(searchTerm) || (t.Description != null && t.Description.Contains(searchTerm)));
    }

    protected override IQueryable<HabitTemplate> ApplySorting(IQueryable<HabitTemplate> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
            "status" => descending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "sortorder" => descending ? query.OrderByDescending(t => t.SortOrder) : query.OrderBy(t => t.SortOrder),
            "usagecount" => descending ? query.OrderByDescending(t => t.UsageCount) : query.OrderBy(t => t.UsageCount),
            "createdat" => descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };
    }
}
