using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class StatsService : IStatsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICheckInService _checkInService;

    public StatsService(IUnitOfWork unitOfWork, ICheckInService checkInService)
    {
        _unitOfWork = unitOfWork;
        _checkInService = checkInService;
    }

    public async Task<StatsOverviewDto> GetOverviewAsync()
    {
        var allUsers = await _unitOfWork.Users.GetAllAsync();
        var allContracts = await _unitOfWork.Contracts.GetAllAsync();
        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        var allViolations = await _unitOfWork.ContractViolations.GetAllAsync();

        var totalContracts = allContracts.Count();
        var completedContracts = allContracts.Count(c => c.Status == ContractStatus.Completed);

        var completedRate = totalContracts > 0
            ? Math.Round((double)completedContracts / totalContracts * 100, 2)
            : 0;

        var violationTypeBreakdown = CalculateViolationTypeBreakdown(allViolations.ToList());

        return new StatsOverviewDto
        {
            TotalUsers = allUsers.Count(),
            TotalContracts = totalContracts,
            ActiveContracts = allContracts.Count(c => c.Status == ContractStatus.Active),
            TotalCheckIns = allCheckIns.Count(),
            TotalViolations = allViolations.Count(),
            CompletedRate = completedRate,
            ViolationTypeBreakdown = violationTypeBreakdown
        };
    }

    public async Task<List<StatsTrendDto>> GetTrendAsync(DateTime? startDate, DateTime? endDate)
    {
        var end = endDate ?? DateTime.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        if (start >= end)
        {
            throw new HabitContract.Domain.Common.BusinessException("开始日期必须早于结束日期");
        }

        var allContracts = await _unitOfWork.Contracts.GetAllAsync();
        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        var allViolations = await _unitOfWork.ContractViolations.GetAllAsync();

        var contractsInRange = allContracts.Where(c => c.CreatedAt >= start && c.CreatedAt <= end).ToList();
        var checkInsInRange = allCheckIns.Where(ci => ci.CheckInDate >= start && ci.CheckInDate <= end).ToList();
        var violationsInRange = allViolations.Where(v => v.ViolationDate >= start && v.ViolationDate <= end).ToList();

        var trends = new List<StatsTrendDto>();
        for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            var nextDay = date.AddDays(1);
            trends.Add(new StatsTrendDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                CheckInCount = checkInsInRange.Count(ci => ci.CheckInDate >= date && ci.CheckInDate < nextDay),
                ViolationCount = violationsInRange.Count(v => v.ViolationDate >= date && v.ViolationDate < nextDay),
                NewContractCount = contractsInRange.Count(c => c.CreatedAt >= date && c.CreatedAt < nextDay)
            });
        }

        return trends;
    }

    public async Task<ContractStatsDto> GetContractStatsAsync(int contractId, int userId)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
        if (contract == null)
        {
            throw new HabitContract.Domain.Common.BusinessException("契约不存在", 404);
        }

        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        var contractCheckIns = allCheckIns
            .Where(ci => ci.ContractId == contractId && ci.UserId == userId)
            .ToList();

        var allViolations = await _unitOfWork.ContractViolations.GetAllAsync();
        var contractViolations = allViolations
            .Where(v => v.ContractId == contractId && v.UserId == userId)
            .ToList();

        var totalDays = (contract.EndDate.Date - contract.StartDate.Date).Days + 1;
        var completedDays = contractCheckIns.Count(ci => ci.Status == CheckInStatus.Normal || ci.Status == CheckInStatus.MakeUp);
        var completionRate = totalDays > 0
            ? Math.Round((double)completedDays / totalDays * 100, 2)
            : 0;

        var streaks = await _checkInService.GetStreaksAsync(contractId, userId);
        var violationTypeBreakdown = CalculateViolationTypeBreakdown(contractViolations);
        var suggestions = GenerateImprovementSuggestions(contractViolations, contractCheckIns);

        return new ContractStatsDto
        {
            ContractId = contractId,
            ContractName = contract.HabitName,
            TotalCheckIns = contractCheckIns.Count,
            NormalCheckIns = contractCheckIns.Count(ci => ci.Status == CheckInStatus.Normal),
            MakeUpCheckIns = contractCheckIns.Count(ci => ci.Status == CheckInStatus.MakeUp),
            MissedCheckIns = contractCheckIns.Count(ci => ci.Status == CheckInStatus.Missed),
            PendingCheckIns = contractCheckIns.Count(ci => ci.Status == CheckInStatus.Pending),
            CurrentStreak = streaks.CurrentStreak,
            LongestStreak = streaks.LongestStreak,
            CompletionRate = completionRate,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            TotalDays = totalDays,
            CompletedDays = completedDays,
            TotalViolations = contractViolations.Count,
            ViolationTypeBreakdown = violationTypeBreakdown,
            ImprovementSuggestions = suggestions
        };
    }

    public async Task<UserStatsDto> GetUserStatsAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new HabitContract.Domain.Common.BusinessException("用户不存在", 404);
        }

        var allContracts = await _unitOfWork.Contracts.GetAllAsync();
        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        var allViolations = await _unitOfWork.ContractViolations.GetAllAsync();

        var partnerContractIds = allPartners
            .Where(p => p.PartnerId == userId && p.Status == PartnerStatus.Accepted)
            .Select(p => p.ContractId)
            .ToList();

        var ownedContractIds = allContracts
            .Where(c => c.OwnerId == userId)
            .Select(c => c.Id)
            .ToList();

        var allUserContractIds = partnerContractIds.Union(ownedContractIds).Distinct().ToList();

        var userContracts = allContracts
            .Where(c => allUserContractIds.Contains(c.Id))
            .ToList();

        var userCheckIns = allCheckIns
            .Where(ci => ci.UserId == userId && allUserContractIds.Contains(ci.ContractId))
            .ToList();

        var userViolations = allViolations
            .Where(v => v.UserId == userId && allUserContractIds.Contains(v.ContractId))
            .ToList();

        var contractStatsList = new List<UserContractStatsDto>();
        int currentMaxStreak = 0;
        int longestMaxStreak = 0;
        int totalCompletedDays = 0;
        int totalRequiredDays = 0;

        foreach (var contract in userContracts)
        {
            var streaks = await _checkInService.GetStreaksAsync(contract.Id, userId);
            var contractCheckIns = userCheckIns.Where(ci => ci.ContractId == contract.Id).ToList();
            var completedDays = contractCheckIns.Count(ci => ci.Status == CheckInStatus.Normal || ci.Status == CheckInStatus.MakeUp);
            var totalDays = (contract.EndDate.Date - contract.StartDate.Date).Days + 1;
            var completionRate = totalDays > 0
                ? Math.Round((double)completedDays / totalDays * 100, 2)
                : 0;

            contractStatsList.Add(new UserContractStatsDto
            {
                ContractId = contract.Id,
                ContractName = contract.HabitName,
                CurrentStreak = streaks.CurrentStreak,
                LongestStreak = streaks.LongestStreak,
                CompletionRate = completionRate
            });

            if (streaks.CurrentStreak > currentMaxStreak)
                currentMaxStreak = streaks.CurrentStreak;
            if (streaks.LongestStreak > longestMaxStreak)
                longestMaxStreak = streaks.LongestStreak;

            totalCompletedDays += completedDays;
            totalRequiredDays += totalDays;
        }

        var overallCompletionRate = totalRequiredDays > 0
            ? Math.Round((double)totalCompletedDays / totalRequiredDays * 100, 2)
            : 0;

        var violationTypeBreakdown = CalculateViolationTypeBreakdown(userViolations);
        var suggestions = GenerateImprovementSuggestions(userViolations, userCheckIns);

        return new UserStatsDto
        {
            UserId = userId,
            Username = user.Username,
            TotalContracts = userContracts.Count,
            ActiveContracts = userContracts.Count(c => c.Status == ContractStatus.Active),
            CompletedContracts = userContracts.Count(c => c.Status == ContractStatus.Completed),
            TotalCheckIns = userCheckIns.Count,
            NormalCheckIns = userCheckIns.Count(ci => ci.Status == CheckInStatus.Normal),
            MakeUpCheckIns = userCheckIns.Count(ci => ci.Status == CheckInStatus.MakeUp),
            MissedCheckIns = userCheckIns.Count(ci => ci.Status == CheckInStatus.Missed),
            CurrentMaxStreak = currentMaxStreak,
            LongestMaxStreak = longestMaxStreak,
            OverallCompletionRate = overallCompletionRate,
            TotalViolations = userViolations.Count,
            ViolationTypeBreakdown = violationTypeBreakdown,
            ImprovementSuggestions = suggestions,
            ContractStats = contractStatsList
        };
    }

    private static List<ViolationTypeStatsDto> CalculateViolationTypeBreakdown(List<Domain.Entities.ContractViolation> violations)
    {
        var total = violations.Count;
        var result = new List<ViolationTypeStatsDto>();

        var activeTypes = new[] { ViolationType.MissedCheckIn, ViolationType.NotMetTarget, ViolationType.Other };

        foreach (var type in activeTypes)
        {
            var count = violations.Count(v => ViolationTypeMigrator.Normalize(v.ViolationType) == type);
            var severeCount = violations.Count(v => ViolationTypeMigrator.Normalize(v.ViolationType) == type && v.IsSevere);
            var percentage = total > 0 ? Math.Round((double)count / total * 100, 2) : 0;

            result.Add(new ViolationTypeStatsDto
            {
                Type = type,
                TypeName = GetViolationTypeName(type),
                Count = count,
                Percentage = percentage,
                SevereCount = severeCount
            });
        }

        return result;
    }

    private static List<ImprovementSuggestionDto> GenerateImprovementSuggestions(
        List<Domain.Entities.ContractViolation> violations,
        List<Domain.Entities.CheckIn> checkIns)
    {
        var suggestions = new List<ImprovementSuggestionDto>();
        var typeGroups = violations.GroupBy(v => ViolationTypeMigrator.Normalize(v.ViolationType)).ToList();

        foreach (var group in typeGroups.OrderByDescending(g => g.Count()))
        {
            var type = group.Key;
            var count = group.Count();
            var suggestion = GetImprovementSuggestion(type, count, checkIns);

            if (!string.IsNullOrEmpty(suggestion))
            {
                suggestions.Add(new ImprovementSuggestionDto
                {
                    ViolationType = type,
                    TypeName = GetViolationTypeName(type),
                    Suggestion = suggestion,
                    ViolationCount = count
                });
            }
        }

        return suggestions;
    }

    private static string GetImprovementSuggestion(ViolationType type, int count, List<Domain.Entities.CheckIn> checkIns)
    {
        return type switch
        {
            ViolationType.MissedCheckIn => count switch
            {
                >= 5 => "频繁忘记打卡，建议设置每日定时提醒，或将打卡时间安排在固定日常习惯之后。",
                >= 3 => "近期有多日未打卡，建议设置提醒通知，或与监督伙伴约定每日打卡时段。",
                _ => "偶尔忘记打卡，建议设置手机提醒，养成每日固定时间打卡的习惯。"
            },
            ViolationType.NotMetTarget => count switch
            {
                >= 5 => "频繁未达标，建议重新评估目标难度，适当降低频率要求，逐步建立习惯。",
                >= 3 => "有多次未达标记录，建议检视当前目标是否合理，或调整打卡策略。",
                _ => "偶尔未达标，可尝试分解目标，每次完成小目标，逐步累积。"
            },
            ViolationType.Other => count switch
            {
                >= 3 => "存在较多其他类型违约，建议仔细阅读契约规则，确保理解所有要求。",
                _ => "有少量其他类型违约，建议关注契约规则细节，避免类似问题。"
            },
            _ => string.Empty
        };
    }

    private static string GetViolationTypeName(ViolationType type)
    {
        return type switch
        {
            ViolationType.MissedCheckIn => "未打卡",
            ViolationType.NotMetTarget => "未达标",
            ViolationType.Other => "其他",
            _ => "未知"
        };
    }
}
