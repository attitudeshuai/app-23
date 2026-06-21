using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class HabitTemplateVersionRepository : Repository<HabitTemplateVersion, int>, IHabitTemplateVersionRepository
{
    public HabitTemplateVersionRepository(HabitContractDbContext context) : base(context)
    {
    }

    public async Task<List<HabitTemplateVersion>> GetByTemplateIdAsync(int templateId)
    {
        return await _dbSet
            .Where(v => v.TemplateId == templateId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }
}
