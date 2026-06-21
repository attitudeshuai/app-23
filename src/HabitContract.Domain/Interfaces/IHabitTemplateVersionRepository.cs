using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IHabitTemplateVersionRepository : IRepository<HabitTemplateVersion, int>
{
    Task<List<HabitTemplateVersion>> GetByTemplateIdAsync(int templateId);
}
