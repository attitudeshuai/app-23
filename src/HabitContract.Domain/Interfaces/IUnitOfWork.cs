using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User, int> Users { get; }
    IRepository<Contract, int> Contracts { get; }
    IRepository<ContractPartner, int> ContractPartners { get; }
    ICheckInRepository CheckIns { get; }
    IMakeUpRequestRepository MakeUpRequests { get; }
    IRepository<ContractViolation, int> ContractViolations { get; }
    IRoleChangeAuditRepository RoleChangeAudits { get; }
    IHabitTemplateCategoryRepository HabitTemplateCategories { get; }
    IHabitTemplateRepository HabitTemplates { get; }
    IHabitTemplateVersionRepository HabitTemplateVersions { get; }
    IContractReminderSettingRepository ContractReminderSettings { get; }
    IReminderRecordRepository ReminderRecords { get; }
    IReminderTemplateRepository ReminderTemplates { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
