using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Interfaces;

public interface IHabitTemplateRepository : IRepository<HabitTemplate, int>
{
    Task<List<HabitTemplate>> GetByCategoryIdAsync(int categoryId);
    Task<List<HabitTemplate>> GetPublishedTemplatesAsync();
    Task<List<HabitTemplate>> GetPublishedByCategoryIdAsync(int categoryId);
    Task<List<HabitTemplate>> GetRecommendedTemplatesAsync(int userId, int count);
}
