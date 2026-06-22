using Microsoft.EntityFrameworkCore.Storage;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;
using HabitContract.Infrastructure.Repositories;

namespace HabitContract.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly HabitContractDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<User, int>? _users;
    private IRepository<Contract, int>? _contracts;
    private IRepository<ContractPartner, int>? _contractPartners;
    private ICheckInRepository? _checkIns;
    private IMakeUpRequestRepository? _makeUpRequests;
    private IRepository<ContractViolation, int>? _contractViolations;
    private IRoleChangeAuditRepository? _roleChangeAudits;
    private IHabitTemplateCategoryRepository? _habitTemplateCategories;
    private IHabitTemplateRepository? _habitTemplates;
    private IHabitTemplateVersionRepository? _habitTemplateVersions;
    private IContractReminderSettingRepository? _contractReminderSettings;
    private IReminderRecordRepository? _reminderRecords;
    private IReminderTemplateRepository? _reminderTemplates;

    public UnitOfWork(HabitContractDbContext context)
    {
        _context = context;
    }

    public IRepository<User, int> Users => _users ??= new UserRepository(_context);
    public IRepository<Contract, int> Contracts => _contracts ??= new ContractRepository(_context);
    public IRepository<ContractPartner, int> ContractPartners => _contractPartners ??= new ContractPartnerRepository(_context);
    public ICheckInRepository CheckIns => _checkIns ??= new CheckInRepository(_context);
    public IMakeUpRequestRepository MakeUpRequests => _makeUpRequests ??= new MakeUpRequestRepository(_context);
    public IRepository<ContractViolation, int> ContractViolations => _contractViolations ??= new ContractViolationRepository(_context);
    public IRoleChangeAuditRepository RoleChangeAudits => _roleChangeAudits ??= new RoleChangeAuditRepository(_context);
    public IHabitTemplateCategoryRepository HabitTemplateCategories => _habitTemplateCategories ??= new HabitTemplateCategoryRepository(_context);
    public IHabitTemplateRepository HabitTemplates => _habitTemplates ??= new HabitTemplateRepository(_context);
    public IHabitTemplateVersionRepository HabitTemplateVersions => _habitTemplateVersions ??= new HabitTemplateVersionRepository(_context);
    public IContractReminderSettingRepository ContractReminderSettings => _contractReminderSettings ??= new ContractReminderSettingRepository(_context);
    public IReminderRecordRepository ReminderRecords => _reminderRecords ??= new ReminderRecordRepository(_context);
    public IReminderTemplateRepository ReminderTemplates => _reminderTemplates ??= new ReminderTemplateRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
