using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IContractPartnerRepository : IRepository<ContractPartner, int>
{
    Task<List<ContractPartner>> GetByContractIdAsync(int contractId);
    Task<List<ContractPartner>> GetByPartnerIdAsync(int partnerId);
}
