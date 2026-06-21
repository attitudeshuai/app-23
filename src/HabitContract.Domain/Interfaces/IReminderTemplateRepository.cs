using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Interfaces;

public interface IReminderTemplateRepository
{
    Task<IEnumerable<ReminderTemplate>> GetAllAsync();

    Task<ReminderTemplate?> GetByIdAsync(int id);

    Task<ReminderTemplate?> GetByTypeAsync(ReminderTemplateType type);

    Task<ReminderTemplate?> GetDefaultAsync(ReminderTemplateType type);

    Task<ReminderTemplate> AddAsync(ReminderTemplate template);

    Task UpdateAsync(ReminderTemplate template);

    Task DeleteAsync(ReminderTemplate template);
}
