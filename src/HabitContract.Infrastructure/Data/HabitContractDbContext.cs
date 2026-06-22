using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;

namespace HabitContract.Infrastructure.Data;

public class HabitContractDbContext : DbContext
{
    public HabitContractDbContext(DbContextOptions<HabitContractDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractPartner> ContractPartners { get; set; }
    public DbSet<CheckIn> CheckIns { get; set; }
    public DbSet<ContractViolation> ContractViolations { get; set; }
    public DbSet<HabitTemplateCategory> HabitTemplateCategories { get; set; }
    public DbSet<HabitTemplate> HabitTemplates { get; set; }
    public DbSet<HabitTemplateVersion> HabitTemplateVersions { get; set; }
    public DbSet<ContractReminderSetting> ContractReminderSettings { get; set; }
    public DbSet<ReminderRecord> ReminderRecords { get; set; }
    public DbSet<ReminderTemplate> ReminderTemplates { get; set; }
    public DbSet<RoleChangeAudit> RoleChangeAudits { get; set; }
    public DbSet<MakeUpRequest> MakeUpRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 用户表配置：用户名和邮箱唯一索引，用于登录和注册校验
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Avatar).HasMaxLength(500);
        });

        // 契约表配置：一个用户可拥有多个契约，删除时限制级联
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasOne(c => c.Owner)
                .WithMany(u => u.Contracts)
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.Template)
                .WithMany()
                .HasForeignKey(c => c.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.Property(c => c.HabitName).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Frequency).IsRequired().HasMaxLength(50);
            entity.Property(c => c.PenaltyDescription).HasMaxLength(500);
            entity.Property(c => c.Status).HasDefaultValue(ContractStatus.Active);
            entity.Property(c => c.CheckInDeadline).HasDefaultValue(new TimeSpan(23, 59, 59));
            entity.Property(c => c.TimeZone).HasDefaultValue("Asia/Shanghai").HasMaxLength(50);
            entity.Property(c => c.MakeUpDeadlineDays).HasDefaultValue(7);
            entity.HasIndex(c => c.OwnerId);
            entity.HasIndex(c => c.TemplateId);
            entity.HasIndex(c => c.Status);
            entity.HasIndex(c => c.StartDate);
            entity.HasIndex(c => c.EndDate);
        });

        // 契约伙伴表配置：同一契约同一伙伴不可重复
        modelBuilder.Entity<ContractPartner>(entity =>
        {
            entity.HasIndex(cp => new { cp.ContractId, cp.PartnerId }).IsUnique();
            entity.HasOne(cp => cp.Contract)
                .WithMany(c => c.Partners)
                .HasForeignKey(cp => cp.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cp => cp.Partner)
                .WithMany(u => u.ContractPartners)
                .HasForeignKey(cp => cp.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(cp => cp.Status).HasDefaultValue(PartnerStatus.Pending);
        });

        // 打卡记录表配置：同一契约同一天不允许重复打卡
        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.HasOne(ci => ci.Contract)
                .WithMany(c => c.CheckIns)
                .HasForeignKey(ci => ci.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ci => ci.User)
                .WithMany(u => u.CheckIns)
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ci => ci.MakeUpRequest)
                .WithMany(m => m.CheckIns)
                .HasForeignKey(ci => ci.MakeUpRequestId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.Property(ci => ci.ProofText).HasMaxLength(1000);
            entity.Property(ci => ci.ProofPhoto).HasMaxLength(500);
            entity.Property(ci => ci.Status).HasDefaultValue(CheckInStatus.Pending);
            entity.Property(ci => ci.ConsecutiveDays).HasDefaultValue(0);
            entity.HasIndex(ci => ci.ContractId);
            entity.HasIndex(ci => ci.UserId);
            entity.HasIndex(ci => ci.CheckInDate);
            entity.HasIndex(ci => ci.Status);
            entity.HasIndex(ci => new { ci.ContractId, ci.UserId, ci.CheckInDate }).IsUnique();
        });

        // 补卡申请表配置
        modelBuilder.Entity<MakeUpRequest>(entity =>
        {
            entity.HasOne(m => m.Contract)
                .WithMany(c => c.MakeUpRequests)
                .HasForeignKey(m => m.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.Reviewer)
                .WithMany()
                .HasForeignKey(m => m.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(m => m.Reason).IsRequired().HasMaxLength(500);
            entity.Property(m => m.ProofText).HasMaxLength(1000);
            entity.Property(m => m.ProofPhoto).HasMaxLength(500);
            entity.Property(m => m.RejectionReason).HasMaxLength(500);
            entity.Property(m => m.Status).HasDefaultValue(MakeUpRequestStatus.Pending);
            entity.HasIndex(m => m.ContractId);
            entity.HasIndex(m => m.UserId);
            entity.HasIndex(m => m.Status);
            entity.HasIndex(m => m.CheckInDate);
            entity.HasIndex(m => new { m.ContractId, m.UserId, m.CheckInDate }).IsUnique();
        });

        // 违约记录表配置：记录契约违约事件
        modelBuilder.Entity<ContractViolation>(entity =>
        {
            entity.HasOne(cv => cv.Contract)
                .WithMany(c => c.Violations)
                .HasForeignKey(cv => cv.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cv => cv.User)
                .WithMany()
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(cv => cv.ViolationType).HasDefaultValue(ViolationType.Other);
            entity.Property(cv => cv.IsSevere).HasDefaultValue(false);
            entity.Property(cv => cv.Reason).IsRequired().HasMaxLength(500);
            entity.Property(cv => cv.IsConfirmed).HasDefaultValue(false);
            entity.HasIndex(cv => cv.ContractId);
            entity.HasIndex(cv => cv.UserId);
            entity.HasIndex(cv => cv.ViolationDate);
            entity.HasIndex(cv => cv.ViolationType);
            entity.HasIndex(cv => cv.IsSevere);
        });

        // 习惯模板分类配置
        modelBuilder.Entity<HabitTemplateCategory>(entity =>
        {
            entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Description).HasMaxLength(200);
            entity.Property(c => c.Icon).HasMaxLength(500);
            entity.Property(c => c.IsActive).HasDefaultValue(true);
            entity.Property(c => c.SortOrder).HasDefaultValue(0);
            entity.HasIndex(c => c.IsActive);
            entity.HasIndex(c => c.SortOrder);
        });

        // 习惯模板配置
        modelBuilder.Entity<HabitTemplate>(entity =>
        {
            entity.HasOne(t => t.Category)
                .WithMany(c => c.Templates)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.Property(t => t.Icon).HasMaxLength(500);
            entity.Property(t => t.DefaultFrequency).IsRequired().HasMaxLength(50);
            entity.Property(t => t.DefaultGoalDescription).IsRequired().HasMaxLength(500);
            entity.Property(t => t.DefaultSupervisorRule).IsRequired().HasMaxLength(500);
            entity.Property(t => t.DefaultPenaltyDescription).HasMaxLength(500);
            entity.Property(t => t.Version).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Status).HasDefaultValue(TemplateStatus.Draft);
            entity.Property(t => t.SortOrder).HasDefaultValue(0);
            entity.Property(t => t.UsageCount).HasDefaultValue(0);
            entity.Property(t => t.CompletionCount).HasDefaultValue(0);
            entity.HasIndex(t => t.CategoryId);
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.SortOrder);
        });

        // 模板版本历史配置
        modelBuilder.Entity<HabitTemplateVersion>(entity =>
        {
            entity.HasOne(v => v.Template)
                .WithMany()
                .HasForeignKey(v => v.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(v => v.Version).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Name).IsRequired().HasMaxLength(100);
            entity.Property(v => v.Description).HasMaxLength(500);
            entity.Property(v => v.DefaultFrequency).IsRequired().HasMaxLength(50);
            entity.Property(v => v.DefaultGoalDescription).IsRequired().HasMaxLength(500);
            entity.Property(v => v.DefaultSupervisorRule).IsRequired().HasMaxLength(500);
            entity.Property(v => v.DefaultPenaltyDescription).HasMaxLength(500);
            entity.Property(v => v.ChangeLog).HasMaxLength(1000);
            entity.HasIndex(v => v.TemplateId);
            entity.HasIndex(v => v.Version);
        });

        modelBuilder.Entity<ContractReminderSetting>(entity =>
        {
            entity.HasOne(s => s.Contract)
                .WithMany()
                .HasForeignKey(s => s.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(s => new { s.ContractId, s.UserId }).IsUnique();
            entity.HasIndex(s => s.UserId);
            entity.HasIndex(s => s.IsEnabled);
        });

        modelBuilder.Entity<ReminderRecord>(entity =>
        {
            entity.HasOne(r => r.Contract)
                .WithMany()
                .HasForeignKey(r => r.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Setting)
                .WithMany(s => s.ReminderRecords)
                .HasForeignKey(r => r.SettingId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.Property(r => r.Title).IsRequired().HasMaxLength(200);
            entity.Property(r => r.Content).IsRequired().HasMaxLength(1000);
            entity.Property(r => r.ErrorMessage).HasMaxLength(500);
            entity.Property(r => r.ContractInfo).HasMaxLength(500);
            entity.HasIndex(r => r.ContractId);
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.ReminderDate);
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => new { r.ContractId, r.UserId, r.ReminderDate });
        });

        modelBuilder.Entity<ReminderTemplate>(entity =>
        {
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.Property(t => t.TitleTemplate).IsRequired().HasMaxLength(200);
            entity.Property(t => t.ContentTemplate).IsRequired().HasMaxLength(1000);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.HasIndex(t => t.Type);
            entity.HasIndex(t => t.IsActive);
            entity.HasIndex(t => t.IsDefault);
        });

        modelBuilder.Entity<RoleChangeAudit>(entity =>
        {
            entity.HasOne<Contract>()
                .WithMany()
                .HasForeignKey(a => a.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(a => a.ChangeReason).HasMaxLength(500);
            entity.Property(a => a.ChangedByUsername).HasMaxLength(50);
            entity.Property(a => a.PartnerUsername).HasMaxLength(50);
            entity.Property(a => a.ContractName).HasMaxLength(100);
            entity.HasIndex(a => a.ContractId);
            entity.HasIndex(a => a.PartnerId);
            entity.HasIndex(a => a.ChangedByUserId);
            entity.HasIndex(a => a.CreatedAt);
        });
    }
}
