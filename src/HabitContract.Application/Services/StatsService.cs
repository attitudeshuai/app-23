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

        return new StatsOverviewDto
        {
            TotalUsers = allUsers.Count(),
            TotalContracts = totalContracts,
            ActiveContracts = allContracts.Count(c => c.Status == ContractStatus.Active),
            TotalCheckIns = allCheckIns.Count(),
            TotalViolations = allViolations.Count(),
            CompletedRate = completedRate
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

        var totalDays = (contract.EndDate.Date - contract.StartDate.Date).Days + 1;
        var completedDays = contractCheckIns.Count(ci => ci.Status == CheckInStatus.Normal || ci.Status == CheckInStatus.MakeUp);
        var completionRate = totalDays > 0
            ? Math.Round((double)completedDays / totalDays * 100, 2)
            : 0;

        var streaks = await _checkInService.GetStreaksAsync(contractId, userId);

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
            CompletedDays = completedDays
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
            ContractStats = contractStatsList
        };
    }
}
