using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User, int> Users { get; }
    IRepository<Contract, int> Contracts { get; }
    IRepository<ContractPartner, int> ContractPartners { get; }
    IRepository<CheckIn, int> CheckIns { get; }
    IRepository<ContractViolation, int> ContractViolations { get; }
    IHabitTemplateCategoryRepository HabitTemplateCategories { get; }
    IHabitTemplateRepository HabitTemplates { get; }
    IHabitTemplateVersionRepository HabitTemplateVersions { get; }
    IContractReminderSettingRepository ContractReminderSettings { get; }
    IReminderRecordRepository ReminderRecords { get; }
    IReminderTemplateRepository ReminderTemplates { get; }
    Task<int> SaveChangesAsync();
}
