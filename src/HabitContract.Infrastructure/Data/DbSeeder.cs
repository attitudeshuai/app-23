using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;

namespace HabitContract.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(HabitContractDbContext context)
    {
        // 按顺序播种数据：先用户，再契约，然后是伙伴、打卡、违约
        if (!await context.Users.AnyAsync())
        {
            await SeedUsersAsync(context);
        }

        if (!await context.Contracts.AnyAsync())
        {
            await SeedContractsAsync(context);
        }

        if (!await context.ContractPartners.AnyAsync())
        {
            await SeedContractPartnersAsync(context);
        }

        if (!await context.CheckIns.AnyAsync())
        {
            await SeedCheckInsAsync(context);
        }

        if (!await context.ContractViolations.AnyAsync())
        {
            await SeedContractViolationsAsync(context);
        }
    }

    private static async Task SeedUsersAsync(HabitContractDbContext context)
    {
        // 使用 PasswordHasher 对用户密码进行哈希处理，确保安全性
        var passwordHasher = new PasswordHasher<User>();

        var users = new List<User>
        {
            new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = passwordHasher.HashPassword(null!, "Admin@123"),
                Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=admin",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                Username = "alice",
                Email = "alice@example.com",
                PasswordHash = passwordHasher.HashPassword(null!, "Alice@123"),
                Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=alice",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new User
            {
                Username = "bob",
                Email = "bob@example.com",
                PasswordHash = passwordHasher.HashPassword(null!, "Bob@123"),
                Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=bob",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new User
            {
                Username = "charlie",
                Email = "charlie@example.com",
                PasswordHash = passwordHasher.HashPassword(null!, "Charlie@123"),
                Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=charlie",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new User
            {
                Username = "diana",
                Email = "diana@example.com",
                PasswordHash = passwordHasher.HashPassword(null!, "Diana@123"),
                Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=diana",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedContractsAsync(HabitContractDbContext context)
    {
        var today = DateTime.UtcNow.Date;

        var contracts = new List<Contract>
        {
            // 活跃契约：晨跑习惯
            new Contract
            {
                OwnerId = 2,
                HabitName = "晨跑",
                Frequency = "每天",
                StartDate = today.AddDays(-10),
                EndDate = today.AddDays(20),
                PenaltyDescription = "违约则给监督伙伴发200元红包",
                Status = ContractStatus.Active,
                CreatedAt = today.AddDays(-10)
            },
            // 活跃契约：阅读习惯
            new Contract
            {
                OwnerId = 3,
                HabitName = "阅读",
                Frequency = "每天",
                StartDate = today.AddDays(-20),
                EndDate = today.AddDays(40),
                PenaltyDescription = "少读一天罚款100元充公",
                Status = ContractStatus.Active,
                CreatedAt = today.AddDays(-20)
            },
            // 已暂停契约：早起打卡
            new Contract
            {
                OwnerId = 4,
                HabitName = "早起",
                Frequency = "每天",
                StartDate = today.AddDays(-5),
                EndDate = today.AddDays(16),
                PenaltyDescription = "起不来就给大家买早餐",
                Status = ContractStatus.Paused,
                CreatedAt = today.AddDays(-5)
            },
            // 已完成契约：健身
            new Contract
            {
                OwnerId = 5,
                HabitName = "健身",
                Frequency = "每周3次",
                StartDate = today.AddDays(-60),
                EndDate = today.AddDays(-4),
                PenaltyDescription = "少去一次发150元红包",
                Status = ContractStatus.Completed,
                CreatedAt = today.AddDays(-60)
            },
            // 已失败契约：戒烟
            new Contract
            {
                OwnerId = 2,
                HabitName = "戒烟",
                Frequency = "每天",
                StartDate = today.AddDays(-30),
                EndDate = today.AddDays(-2),
                PenaltyDescription = "抽烟一次罚款500元",
                Status = ContractStatus.Failed,
                CreatedAt = today.AddDays(-30)
            },
            // 活跃契约：冥想
            new Contract
            {
                OwnerId = 3,
                HabitName = "冥想",
                Frequency = "每天",
                StartDate = today.AddDays(-3),
                EndDate = today.AddDays(27),
                PenaltyDescription = "漏做一天发50元红包",
                Status = ContractStatus.Active,
                CreatedAt = today.AddDays(-3)
            }
        };

        await context.Contracts.AddRangeAsync(contracts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedContractPartnersAsync(HabitContractDbContext context)
    {
        var partners = new List<ContractPartner>
        {
            // 契约1的监督伙伴：已接受
            new ContractPartner
            {
                ContractId = 1,
                PartnerId = 3,
                Status = PartnerStatus.Accepted,
                JoinedAt = DateTime.UtcNow.AddDays(-9),
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            // 契约1的见证伙伴：已接受
            new ContractPartner
            {
                ContractId = 1,
                PartnerId = 4,
                Status = PartnerStatus.Accepted,
                JoinedAt = DateTime.UtcNow.AddDays(-9),
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            // 契约2的监督伙伴：已接受
            new ContractPartner
            {
                ContractId = 2,
                PartnerId = 2,
                Status = PartnerStatus.Accepted,
                JoinedAt = DateTime.UtcNow.AddDays(-19),
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            // 契约3的监督伙伴：待接受
            new ContractPartner
            {
                ContractId = 3,
                PartnerId = 5,
                Status = PartnerStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            // 契约4的监督伙伴：已接受
            new ContractPartner
            {
                ContractId = 4,
                PartnerId = 4,
                Status = PartnerStatus.Accepted,
                JoinedAt = DateTime.UtcNow.AddDays(-58),
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            // 契约5的监督伙伴：已拒绝
            new ContractPartner
            {
                ContractId = 5,
                PartnerId = 5,
                Status = PartnerStatus.Rejected,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            // 契约6的监督伙伴：待接受
            new ContractPartner
            {
                ContractId = 6,
                PartnerId = 4,
                Status = PartnerStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            // 契约6的见证伙伴：已接受
            new ContractPartner
            {
                ContractId = 6,
                PartnerId = 5,
                Status = PartnerStatus.Accepted,
                JoinedAt = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        await context.ContractPartners.AddRangeAsync(partners);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCheckInsAsync(HabitContractDbContext context)
    {
        var today = DateTime.UtcNow.Date;

        var checkIns = new List<CheckIn>
        {
            // 晨跑契约打卡记录
            new CheckIn
            {
                ContractId = 1,
                UserId = 2,
                CheckInDate = today.AddDays(-9),
                ProofText = "今天跑了3.5公里，状态不错",
                ProofPhoto = "https://images.unsplash.com/photo-1552674605-db6ffd4facb5?w=200",
                CreatedAt = today.AddDays(-9)
            },
            new CheckIn
            {
                ContractId = 1,
                UserId = 2,
                CheckInDate = today.AddDays(-8),
                ProofText = "早起跑步打卡，6点出发",
                ProofPhoto = "https://images.unsplash.com/photo-1552674605-db6ffd4facb5?w=200",
                CreatedAt = today.AddDays(-8)
            },
            new CheckIn
            {
                ContractId = 1,
                UserId = 2,
                CheckInDate = today.AddDays(-7),
                ProofText = "3公里完成，配速5分30秒",
                ProofPhoto = "https://images.unsplash.com/photo-1552674605-db6ffd4facb5?w=200",
                CreatedAt = today.AddDays(-7)
            },
            // 阅读契约打卡记录
            new CheckIn
            {
                ContractId = 2,
                UserId = 3,
                CheckInDate = today.AddDays(-10),
                ProofText = "今天读了《原则》第3章，收获很大",
                ProofPhoto = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=200",
                CreatedAt = today.AddDays(-10)
            },
            new CheckIn
            {
                ContractId = 2,
                UserId = 3,
                CheckInDate = today.AddDays(-9),
                ProofText = "阅读1.5小时，超额完成",
                ProofPhoto = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=200",
                CreatedAt = today.AddDays(-9)
            },
            new CheckIn
            {
                ContractId = 2,
                UserId = 3,
                CheckInDate = today.AddDays(-8),
                ProofText = "今天有点忙，只读了50分钟",
                CreatedAt = today.AddDays(-8)
            },
            // 早起契约打卡记录
            new CheckIn
            {
                ContractId = 3,
                UserId = 4,
                CheckInDate = today.AddDays(-4),
                ProofText = "6:30起床，早安！",
                ProofPhoto = "https://images.unsplash.com/photo-1507608616759-54f48f0af0ee?w=200",
                CreatedAt = today.AddDays(-4)
            },
            // 冥想契约打卡记录
            new CheckIn
            {
                ContractId = 6,
                UserId = 3,
                CheckInDate = today.AddDays(-2),
                ProofText = "冥想20分钟，专注力提升",
                CreatedAt = today.AddDays(-2)
            },
            new CheckIn
            {
                ContractId = 6,
                UserId = 3,
                CheckInDate = today.AddDays(-1),
                ProofText = "冥想15分钟，保持日常练习",
                CreatedAt = today.AddDays(-1)
            },
            new CheckIn
            {
                ContractId = 1,
                UserId = 2,
                CheckInDate = today.AddDays(-6),
                ProofText = "下雨天室内跑步3公里",
                ProofPhoto = "https://images.unsplash.com/photo-1552674605-db6ffd4facb5?w=200",
                CreatedAt = today.AddDays(-6)
            }
        };

        await context.CheckIns.AddRangeAsync(checkIns);
        await context.SaveChangesAsync();
    }

    private static async Task SeedContractViolationsAsync(HabitContractDbContext context)
    {
        var today = DateTime.UtcNow.Date;

        var violations = new List<ContractViolation>
        {
            // 晨跑契约：漏打卡违约
            new ContractViolation
            {
                ContractId = 1,
                ViolationDate = today.AddDays(-5),
                Reason = "第5天未打卡，缺少跑步记录",
                IsConfirmed = false,
                CreatedAt = today.AddDays(-5)
            },
            // 阅读契约：时长不足违约
            new ContractViolation
            {
                ContractId = 2,
                ViolationDate = today.AddDays(-8),
                Reason = "阅读时长不足1小时",
                IsConfirmed = true,
                CreatedAt = today.AddDays(-8)
            },
            // 戒烟契约：复吸违约
            new ContractViolation
            {
                ContractId = 5,
                ViolationDate = today.AddDays(-15),
                Reason = "被发现抽烟一次",
                IsConfirmed = true,
                CreatedAt = today.AddDays(-15)
            },
            // 戒烟契约：再次违约
            new ContractViolation
            {
                ContractId = 5,
                ViolationDate = today.AddDays(-10),
                Reason = "连续两天未打卡，疑似复吸",
                IsConfirmed = true,
                CreatedAt = today.AddDays(-10)
            },
            // 晨跑契约：待确认违约
            new ContractViolation
            {
                ContractId = 1,
                ViolationDate = today.AddDays(-3),
                Reason = "打卡照片模糊，无法确认",
                IsConfirmed = false,
                CreatedAt = today.AddDays(-3)
            }
        };

        await context.ContractViolations.AddRangeAsync(violations);
        await context.SaveChangesAsync();
    }
}
