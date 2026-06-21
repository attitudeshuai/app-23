using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class ContractPartnerRepository : Repository<ContractPartner, int>, IContractPartnerRepository
{
    public ContractPartnerRepository(HabitContractDbContext context) : base(context)
    {
    }

    // 根据契约ID查询所有伙伴关系
    public async Task<List<ContractPartner>> GetByContractIdAsync(int contractId)
    {
        return await _dbSet
            .Include(cp => cp.Partner)
            .Where(cp => cp.ContractId == contractId)
            .ToListAsync();
    }

    // 根据伙伴ID查询其参与的所有契约
    public async Task<List<ContractPartner>> GetByPartnerIdAsync(int partnerId)
    {
        return await _dbSet
            .Include(cp => cp.Contract)
            .Where(cp => cp.PartnerId == partnerId)
            .ToListAsync();
    }

    protected override IQueryable<ContractPartner> ApplySearch(IQueryable<ContractPartner> query, string searchTerm)
    {
        return query.Where(cp => cp.Status.ToString().Contains(searchTerm));
    }

    protected override IQueryable<ContractPartner> ApplySorting(IQueryable<ContractPartner> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "status" => descending ? query.OrderByDescending(cp => cp.Status) : query.OrderBy(cp => cp.Status),
            "joinedat" => descending ? query.OrderByDescending(cp => cp.JoinedAt) : query.OrderBy(cp => cp.JoinedAt),
            "createdat" => descending ? query.OrderByDescending(cp => cp.CreatedAt) : query.OrderBy(cp => cp.CreatedAt),
            _ => query.OrderByDescending(cp => cp.CreatedAt)
        };
    }
}
