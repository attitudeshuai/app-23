using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IContractViolationRepository : IRepository<ContractViolation, int>
{
    Task<List<ContractViolation>> GetByContractIdAsync(int contractId);
}
