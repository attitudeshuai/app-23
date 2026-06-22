using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IRoleChangeAuditRepository : IRepository<RoleChangeAudit, int>
{
    Task<List<RoleChangeAudit>> GetByContractIdAsync(int contractId);
    Task<List<RoleChangeAudit>> GetByPartnerIdAsync(int partnerId);
}
