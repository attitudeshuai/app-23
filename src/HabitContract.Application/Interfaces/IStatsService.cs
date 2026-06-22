using HabitContract.Application.DTOs;

namespace HabitContract.Application.Interfaces;

public interface IStatsService
{
    Task<StatsOverviewDto> GetOverviewAsync();
    Task<List<StatsTrendDto>> GetTrendAsync(DateTime? startDate, DateTime? endDate);
    Task<ContractStatsDto> GetContractStatsAsync(int contractId, int userId);
    Task<UserStatsDto> GetUserStatsAsync(int userId);
}
