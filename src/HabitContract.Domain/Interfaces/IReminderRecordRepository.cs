using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IReminderRecordRepository
{
    Task<ReminderRecord?> GetByIdAsync(int id);

    Task<bool> HasSentTodayAsync(int contractId, int userId, DateTime date);

    Task<IEnumerable<ReminderRecord>> GetPendingRemindersAsync();

    Task<IEnumerable<ReminderRecord>> GetByContractIdAsync(int contractId);

    Task<IEnumerable<ReminderRecord>> GetByUserIdAsync(int userId);

    Task<ReminderRecord> AddAsync(ReminderRecord record);

    Task UpdateAsync(ReminderRecord record);

    Task DeleteAsync(ReminderRecord record);
}
