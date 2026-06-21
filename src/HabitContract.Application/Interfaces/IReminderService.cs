using HabitContract.Application.DTOs;
using HabitContract.Domain.Common;

namespace HabitContract.Application.Interfaces;

public interface IReminderService
{
    Task<ReminderSettingDto> CreateSettingAsync(int userId, ReminderSettingCreateDto dto);

    Task<ReminderSettingDto> UpdateSettingAsync(int userId, int id, ReminderSettingUpdateDto dto);

    Task DeleteSettingAsync(int userId, int id);

    Task<ReminderSettingDto> GetSettingByIdAsync(int id);

    Task<IEnumerable<ReminderSettingDto>> GetMySettingsAsync(int userId);

    Task<IEnumerable<ReminderSettingDto>> GetSettingsByContractAsync(int contractId);

    Task DisableSettingAsync(int userId, int contractId);

    Task<IEnumerable<ReminderRecordDto>> GetMyReminderRecordsAsync(int userId, QueryParameters parameters);

    Task<IEnumerable<ReminderRecordDto>> GetReminderRecordsByContractAsync(int contractId, QueryParameters parameters);

    Task SubmitFeedbackAsync(int userId, ReminderFeedbackDto dto);

    Task ProcessRemindersAsync(CancellationToken cancellationToken);
}
