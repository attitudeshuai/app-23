using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class RoleChangeAuditRepository : Repository<RoleChangeAudit, int>, IRoleChangeAuditRepository
{
    public RoleChangeAuditRepository(HabitContractDbContext context) : base(context)
    {
    }

    public async Task<List<RoleChangeAudit>> GetByContractIdAsync(int contractId)
    {
        return await _dbSet
            .Where(a => a.ContractId == contractId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<RoleChangeAudit>> GetByPartnerIdAsync(int partnerId)
    {
        return await _dbSet
            .Where(a => a.PartnerId == partnerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
