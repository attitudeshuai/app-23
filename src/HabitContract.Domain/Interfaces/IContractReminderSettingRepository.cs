using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IContractReminderSettingRepository
{
    Task<IEnumerable<ContractReminderSetting>> GetEnabledSettingsAsync();

    Task<IEnumerable<ContractReminderSetting>> GetByContractIdAsync(int contractId);

    Task<IEnumerable<ContractReminderSetting>> GetByUserIdAsync(int userId);

    Task<ContractReminderSetting?> GetByContractAndUserAsync(int contractId, int userId);

    Task<ContractReminderSetting> AddAsync(ContractReminderSetting setting);

    Task UpdateAsync(ContractReminderSetting setting);

    Task DeleteAsync(ContractReminderSetting setting);
}
