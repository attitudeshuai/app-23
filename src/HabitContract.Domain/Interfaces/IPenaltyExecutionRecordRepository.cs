using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Interfaces;

public interface IPenaltyExecutionRecordRepository : IRepository<PenaltyExecutionRecord, int>
{
    Task<List<PenaltyExecutionRecord>> GetByContractIdAsync(int contractId);

    Task<List<PenaltyExecutionRecord>> GetByUserIdAsync(int userId);

    Task<List<PenaltyExecutionRecord>> GetByContractIdAndUserIdAsync(int contractId, int userId);

    Task<List<PenaltyExecutionRecord>> GetByViolationIdAsync(int violationId);

    Task<List<PenaltyExecutionRecord>> GetTrendByDateRangeAsync(DateTime startDate, DateTime endDate);

    Task<List<PenaltyExecutionRecord>> GetByStatusAsync(PenaltyExecutionStatus status);
}
