using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class HabitTemplateCategoryRepository : Repository<HabitTemplateCategory, int>, IHabitTemplateCategoryRepository
{
    public HabitTemplateCategoryRepository(HabitContractDbContext context) : base(context)
    {
    }

    public async Task<List<HabitTemplateCategory>> GetActiveCategoriesAsync()
    {
        return await _dbSet
            .Include(c => c.Templates)
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public override async Task<HabitTemplateCategory?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Templates)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    protected override IQueryable<HabitTemplateCategory> ApplySearch(IQueryable<HabitTemplateCategory> query, string searchTerm)
    {
        return query.Where(c => c.Name.Contains(searchTerm));
    }

    protected override IQueryable<HabitTemplateCategory> ApplySorting(IQueryable<HabitTemplateCategory> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "sortorder" => descending ? query.OrderByDescending(c => c.SortOrder) : query.OrderBy(c => c.SortOrder),
            "createdat" => descending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.SortOrder)
        };
    }
}
