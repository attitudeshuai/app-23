using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Interfaces;

public interface ICheckInRepository : IRepository<CheckIn, int>
{
    Task<List<CheckIn>> GetByContractIdAsync(int contractId);
    Task<List<CheckIn>> GetByUserIdAsync(int userId);
    Task<List<CheckIn>> GetByContractAndUserIdAsync(int contractId, int userId);
    Task<CheckIn?> GetByDateAsync(int contractId, int userId, DateTime checkInDate);
    Task<List<CheckIn>> GetPendingCheckInsAsync();
    Task<int> GetConsecutiveDaysAsync(int contractId, int userId, DateTime checkInDate);
    Task<(int CurrentStreak, int LongestStreak)> GetStreaksAsync(int contractId, int userId);
    Task UpdateStatusForMissedDeadlinesAsync();
}
