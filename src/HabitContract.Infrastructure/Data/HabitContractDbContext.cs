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
            entity.Property(c => c.HabitName).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Frequency).IsRequired().HasMaxLength(50);
            entity.Property(c => c.PenaltyDescription).HasMaxLength(500);
            entity.Property(c => c.Status).HasDefaultValue(ContractStatus.Active);
            entity.HasIndex(c => c.OwnerId);
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
            entity.Property(ci => ci.ProofText).HasMaxLength(1000);
            entity.Property(ci => ci.ProofPhoto).HasMaxLength(500);
            entity.HasIndex(ci => ci.ContractId);
            entity.HasIndex(ci => ci.UserId);
            entity.HasIndex(ci => ci.CheckInDate);
            entity.HasIndex(ci => new { ci.ContractId, ci.CheckInDate }).IsUnique();
        });

        // 违约记录表配置：记录契约违约事件
        modelBuilder.Entity<ContractViolation>(entity =>
        {
            entity.HasOne(cv => cv.Contract)
                .WithMany(c => c.Violations)
                .HasForeignKey(cv => cv.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(cv => cv.Reason).IsRequired().HasMaxLength(500);
            entity.Property(cv => cv.IsConfirmed).HasDefaultValue(false);
            entity.HasIndex(cv => cv.ContractId);
            entity.HasIndex(cv => cv.ViolationDate);
        });
    }
}
