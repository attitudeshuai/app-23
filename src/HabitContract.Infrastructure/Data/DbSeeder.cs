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

        if (!await context.HabitTemplateCategories.AnyAsync())
        {
            await SeedTemplateCategoriesAsync(context);
        }

        if (!await context.HabitTemplates.AnyAsync())
        {
            await SeedHabitTemplatesAsync(context);
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

    private static async Task SeedTemplateCategoriesAsync(HabitContractDbContext context)
    {
        var categories = new List<HabitTemplateCategory>
        {
            new HabitTemplateCategory
            {
                Name = "运动健身",
                Description = "各类运动健身习惯养成",
                Icon = "🏃",
                SortOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new HabitTemplateCategory
            {
                Name = "学习提升",
                Description = "阅读、学习等自我提升习惯",
                Icon = "📚",
                SortOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new HabitTemplateCategory
            {
                Name = "健康生活",
                Description = "作息、饮食等健康生活习惯",
                Icon = "💪",
                SortOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new HabitTemplateCategory
            {
                Name = "心灵成长",
                Description = "冥想、反思等心灵成长习惯",
                Icon = "🧘",
                SortOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new HabitTemplateCategory
            {
                Name = "戒除不良习惯",
                Description = "戒烟、戒熬夜等戒除类习惯",
                Icon = "🚫",
                SortOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            }
        };

        await context.HabitTemplateCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedHabitTemplatesAsync(HabitContractDbContext context)
    {
        var templates = new List<HabitTemplate>
        {
            new HabitTemplate
            {
                CategoryId = 1,
                Name = "每日晨跑",
                Description = "每天早晨跑步30分钟，增强体质",
                Icon = "🏃‍♂️",
                DefaultFrequency = "每天",
                DefaultDurationDays = 30,
                DefaultGoalDescription = "每天早晨6:30起床，跑步30分钟，距离不少于3公里",
                DefaultSupervisorRule = "监督伙伴每日核查跑步记录，缺卡一次记为违约",
                DefaultPenaltyDescription = "违约一次给监督伙伴发100元红包",
                Version = "1.0.0",
                Status = Domain.Enums.TemplateStatus.Published,
                SortOrder = 1,
                UsageCount = 156,
                CompletionCount = 89,
                CreatedAt = DateTime.UtcNow.AddDays(-50)
            },
            new HabitTemplate
            {
                CategoryId = 1,
                Name = "每周健身3次",
                Description = "每周去健身房3次，增肌塑形",
                Icon = "🏋️",
                DefaultFrequency = "每周3次",
                DefaultDurationDays = 90,
                DefaultGoalDescription = "每周一、三、五去健身房锻炼，每次不少于1小时",
                DefaultSupervisorRule = "监督伙伴核查健身打卡照片，每周少一次记为违约",
                DefaultPenaltyDescription = "少去一次发150元红包",
                Version = "1.0.0",
                Status = Domain.Enums.TemplateStatus.Published,
                SortOrder = 2,
                UsageCount = 203,
                CompletionCount = 112,
                CreatedAt = DateTime.UtcNow.AddDays(-48)
            },
            new HabitTemplate
            {
                CategoryId = 2,
                Name = "每日阅读30分钟",
                Description = "每天阅读30分钟，养成读书习惯",
                Icon = "📖",
                DefaultFrequency = "每天",
                DefaultDurationDays = 30,
                DefaultGoalDescription = "每天阅读至少30分钟，书籍类型不限",
                DefaultSupervisorRule = "监督伙伴每日核查阅读记录和心得",
                DefaultPenaltyDescription = "少读一天罚款50元充公",
                Version = "1.0.0",
                Status = Domain.Enums.TemplateStatus.Published,
                SortOrder = 1,
                UsageCount = 312,
                CompletionCount = 198,
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            },
            new HabitTemplate
            {
                CategoryId = 2,
                Name = "每周读完一本书",
                Description = "每周读完一本好书，提升认知",
                Icon = "📕",
                DefaultFrequency = "每周1本",
                DefaultDurationDays = 90,
                DefaultGoalDescription = "每周读完一本书，周末分享读书笔记",
                DefaultSupervisorRule = "周日晚提交读书笔记，未完成记违约一次",
                DefaultPenaltyDescription = "未完成一周发200元红包",
                Version = "1.0.0",
                Status = Domain.Enums.TemplateStatus.Published,
                SortOrder = 2,
                UsageCount = 178,
                CompletionCount = 95,
                CreatedAt = DateTime.UtcNow.AddDays(-42)
            },
            new HabitTemplate
            {
                CategoryId = 3,
                Name = "早起打卡",
                Description = "每天早起打卡，养成规律作息",
                Icon = "🌅",
                DefaultFrequency = "每天",
                DefaultDurationDays = 21,
                DefaultGoalDescription = "每天早上7点前起床打卡",
                DefaultSupervisorRule = "监督伙伴每日核查早起打卡时间",
                DefaultPenaltyDescription = "起不来就给大家买早餐",
                Version = "1.0.0",
                Status = Domain.Enums.TemplateStatus.Published,
                SortOrder = 1,
                UsageCount = 245,
                CompletionCount = 156,
                CreatedAt = DateTime.UtcNow.AddDays(-40)
            },
            new HabitTemplate
            {
                CategoryId = 3,
                Name = "每天喝8杯水",
                Description = "保持每天充足饮水，促进健康",
                Icon = "💧",
                DefaultFrequency = "每天",
                DefaultDurationDays = 30,
                DefaultGoalDescription = "每天喝8杯水，每杯约250ml",
                DefaultSupervisorRule = "监督伙伴每日核查饮水打卡记录",
                DefaultPenaltyDescription = "少喝一杯罚10元",
                Version = "1.0.0",
                Status = Domain.Enums.TemplateStatus.Published,
                SortOrder = 2,
                UsageCount = 134,
                CompletionCount = 87,
                CreatedAt = DateTime.UtcNow.AddDays(-38)
            },
            new HabitTemplate
            {
                CategoryId = 4,
                Name = "每日冥想15分钟",
                Description = "每天冥想15分钟，提升专注力",
                Icon = "🧘‍♀️",
                DefaultFrequency = "每天",
                DefaultDurationDays = 30,
                DefaultGoalDescription = "每天冥想15分钟，可以使用冥想APP辅助",
                DefaultSupervisorRule = "监督伙伴每日核查冥想记录",
                DefaultPenaltyDescription = "漏做一天发50元红包",
                Version = "1.0.0",
                Status = Domain.Enums.TemplateStatus.Published,
                SortOrder = 1,
                UsageCount = 167,
                CompletionCount = 110,
                CreatedAt = DateTime.UtcNow.AddDays(-35)
            },
            new HabitTemplate
            {
                CategoryId = 4,
                Name = "每日写日记",
                Description = "每天记录生活，反思成长",
                Icon = "📝",
                DefaultFrequency = "每天",
                DefaultDurationDays = 30,
                DefaultGoalDescription = "每天写日记，记录当天的收获和感悟",
                DefaultSupervisorRule = "监督伙伴每周抽查2-3天的日记内容",
                DefaultPenaltyDescription = "缺一天罚30元",
                Version = "1.0.0",
                Status = Domain.Enums.TemplateStatus.Published,
                SortOrder = 2,
                UsageCount = 98,
                CompletionCount = 62,
                CreatedAt = DateTime.UtcNow.AddDays(-32)
            },
            new HabitTemplate
            {
                CategoryId = 5,
                Name = "戒烟挑战",
                Description = "30天戒烟挑战，摆脱烟瘾",
                Icon = "🚭",
                DefaultFrequency = "每天",
                DefaultDurationDays = 30,
                DefaultGoalDescription = "30天完全不抽烟，包括电子烟",
                DefaultSupervisorRule = "监督伙伴每日核查，发现抽烟立即记违约",
                DefaultPenaltyDescription = "抽烟一次罚款500元",
                Version = "1.0.0",
                Status = Domain.Enums.TemplateStatus.Published,
                SortOrder = 1,
                UsageCount = 89,
                CompletionCount = 34,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new HabitTemplate
            {
                CategoryId = 5,
                Name = "不熬夜挑战",
                Description = "21天不熬夜，早睡早起",
                Icon = "😴",
                DefaultFrequency = "每天",
                DefaultDurationDays = 21,
                DefaultGoalDescription = "每天晚上11点前入睡，早上7点前起床",
                DefaultSupervisorRule = "监督伙伴核查入睡和起床时间",
                DefaultPenaltyDescription = "熬夜一次发200元红包",
                Version = "1.0.0",
                Status = Domain.Enums.TemplateStatus.Published,
                SortOrder = 2,
                UsageCount = 145,
                CompletionCount = 78,
                CreatedAt = DateTime.UtcNow.AddDays(-28)
            }
        };

        await context.HabitTemplates.AddRangeAsync(templates);
        await context.SaveChangesAsync();
    }
}
