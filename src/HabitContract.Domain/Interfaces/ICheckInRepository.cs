using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface ICheckInRepository : IRepository<CheckIn, int>
{
    Task<List<CheckIn>> GetByContractIdAsync(int contractId);
    Task<List<CheckIn>> GetByUserIdAsync(int userId);
}
