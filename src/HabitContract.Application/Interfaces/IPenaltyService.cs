using HabitContract.Application.DTOs;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.Interfaces;

public interface IPenaltyRuleParser
{
    PenaltyCalculationResult ParseAndCalculate(
        PenaltyRule? rule,
        Contract contract,
        User user,
        ContractViolation violation,
        int priorViolationCount = 0);

    bool TryParseRuleExpression(string expression, out string parsedDescription);

    string GenerateDefaultDescription(PenaltyType type, PenaltySeverity severity);
}

public interface IPenaltyService
{
    Task<PagedResultDto<PenaltyRuleDto>> GetPenaltyRulesAsync(QueryParameters parameters);
    Task<PenaltyRuleDto?> GetPenaltyRuleByIdAsync(int id);
    Task<List<PenaltyRuleDto>> GetPenaltyRulesByContractIdAsync(int contractId);
    Task<PenaltyRuleDto> CreatePenaltyRuleAsync(int userId, PenaltyRuleCreateDto dto);
    Task<PenaltyRuleDto> UpdatePenaltyRuleAsync(int userId, int id, PenaltyRuleUpdateDto dto);
    Task DeletePenaltyRuleAsync(int userId, int id);

    Task<PagedResultDto<PenaltyExecutionDto>> GetExecutionRecordsAsync(QueryParameters parameters);
    Task<PenaltyExecutionDto?> GetExecutionRecordByIdAsync(int id);
    Task<List<PenaltyExecutionDto>> GetExecutionRecordsByContractIdAsync(int contractId);
    Task<List<PenaltyExecutionDto>> GetExecutionRecordsByUserIdAsync(int userId);
    Task<List<PenaltyExecutionDto>> GetExecutionRecordsByContractIdAndUserIdAsync(int contractId, int userId);
    Task<PenaltyExecutionDto> CreateExecutionRecordAsync(int operatorId, PenaltyExecutionCreateDto dto);
    Task<PenaltyExecutionDto> UpdateExecutionRecordAsync(int operatorId, int id, PenaltyExecutionUpdateDto dto);
    Task<PenaltyExecutionDto> WaiveExecutionRecordAsync(int operatorId, int id, PenaltyExecutionWaiveDto dto);

    Task<PenaltyOverviewDto> GetOverviewAsync();
    Task<List<PenaltyTrendDto>> GetTrendAsync(DateTime? startDate, DateTime? endDate);

    Task<DefaultPenaltyConfigDto> GetDefaultPenaltyConfigAsync(int contractId);
    Task<PenaltyRuleDto> SupplementPenaltyRuleAsync(int adminUserId, int contractId, PenaltyRuleCreateDto dto);
}
