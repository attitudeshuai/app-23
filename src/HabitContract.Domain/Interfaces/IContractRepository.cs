using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IContractRepository : IRepository<Contract, int>
{
    Task<List<Contract>> GetByOwnerIdAsync(int ownerId);
}
