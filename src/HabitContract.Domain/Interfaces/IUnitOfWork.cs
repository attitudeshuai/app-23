using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User, int> Users { get; }
    IRepository<Contract, int> Contracts { get; }
    IRepository<ContractPartner, int> ContractPartners { get; }
    IRepository<CheckIn, int> CheckIns { get; }
    IRepository<ContractViolation, int> ContractViolations { get; }
    Task<int> SaveChangesAsync();
}
