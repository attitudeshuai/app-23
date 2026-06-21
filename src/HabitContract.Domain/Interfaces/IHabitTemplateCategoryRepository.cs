using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IHabitTemplateCategoryRepository : IRepository<HabitTemplateCategory, int>
{
    Task<List<HabitTemplateCategory>> GetActiveCategoriesAsync();
}
