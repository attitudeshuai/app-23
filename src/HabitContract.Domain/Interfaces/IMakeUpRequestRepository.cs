using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Interfaces;

public interface IMakeUpRequestRepository : IRepository<MakeUpRequest, int>
{
    Task<List<MakeUpRequest>> GetByContractIdAsync(int contractId);
    Task<List<MakeUpRequest>> GetByUserIdAsync(int userId);
    Task<List<MakeUpRequest>> GetPendingRequestsAsync();
    Task<MakeUpRequest?> GetByDateAsync(int contractId, int userId, DateTime checkInDate);
}
