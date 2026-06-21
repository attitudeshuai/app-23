using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class StatsService : IStatsService
{
    private readonly IUnitOfWork _unitOfWork;

    public StatsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StatsOverviewDto> GetOverviewAsync()
    {
        var allUsers = await _unitOfWork.Users.GetAllAsync();
        var allContracts = await _unitOfWork.Contracts.GetAllAsync();
        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        var allViolations = await _unitOfWork.ContractViolations.GetAllAsync();

        var totalContracts = allContracts.Count();
        var completedContracts = allContracts.Count(c => c.Status == ContractStatus.Completed);

        // 完成率 = 已完成契约数 / 总契约数
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
        // 默认查询最近30天
        var end = endDate ?? DateTime.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        if (start >= end)
        {
            throw new HabitContract.Domain.Common.BusinessException("开始日期必须早于结束日期");
        }

        var allContracts = await _unitOfWork.Contracts.GetAllAsync();
        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        var allViolations = await _unitOfWork.ContractViolations.GetAllAsync();

        // 过滤在日期范围内的数据
        var contractsInRange = allContracts.Where(c => c.CreatedAt >= start && c.CreatedAt <= end).ToList();
        var checkInsInRange = allCheckIns.Where(ci => ci.CheckInDate >= start && ci.CheckInDate <= end).ToList();
        var violationsInRange = allViolations.Where(v => v.ViolationDate >= start && v.ViolationDate <= end).ToList();

        // 按日期分组统计
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
}
