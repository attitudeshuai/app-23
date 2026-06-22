using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class PenaltyService : IPenaltyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPenaltyRuleParser _penaltyRuleParser;
    private readonly IPermissionService _permissionService;

    public PenaltyService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPenaltyRuleParser penaltyRuleParser,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _penaltyRuleParser = penaltyRuleParser;
        _permissionService = permissionService;
    }

    public async Task<PagedResultDto<PenaltyRuleDto>> GetPenaltyRulesAsync(QueryParameters parameters)
    {
        var pagedResult = await _unitOfWork.PenaltyRules.GetPagedAsync(parameters);
        return await MapToPagedRuleDto(pagedResult);
    }

    public async Task<PenaltyRuleDto?> GetPenaltyRuleByIdAsync(int id)
    {
        var rule = await _unitOfWork.PenaltyRules.GetByIdAsync(id);
        if (rule == null) return null;
        return await MapToRuleDto(rule);
    }

    public async Task<List<PenaltyRuleDto>> GetPenaltyRulesByContractIdAsync(int contractId)
    {
        var rules = await _unitOfWork.PenaltyRules.GetByContractIdAsync(contractId);
        var dtos = new List<PenaltyRuleDto>();
        foreach (var rule in rules)
        {
            dtos.Add(await MapToRuleDto(rule));
        }
        return dtos;
    }

    public async Task<PenaltyRuleDto> CreatePenaltyRuleAsync(int userId, PenaltyRuleCreateDto dto)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(dto.ContractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, dto.ContractId, ContractOperation.EditCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        var rule = _mapper.Map<PenaltyRule>(dto);
        rule.IsActive = true;
        rule.CreatedAt = DateTime.UtcNow;

        var created = await _unitOfWork.PenaltyRules.AddAsync(rule);
        await _unitOfWork.SaveChangesAsync();

        return await MapToRuleDto(created);
    }

    public async Task<PenaltyRuleDto> UpdatePenaltyRuleAsync(int userId, int id, PenaltyRuleUpdateDto dto)
    {
        var rule = await _unitOfWork.PenaltyRules.GetByIdAsync(id);
        if (rule == null)
        {
            throw new BusinessException("惩罚规则不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, rule.ContractId, ContractOperation.EditCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        if (dto.PenaltyType.HasValue)
            rule.PenaltyType = dto.PenaltyType.Value;
        if (dto.DefaultSeverity.HasValue)
            rule.DefaultSeverity = dto.DefaultSeverity.Value;
        if (dto.RuleExpression != null)
            rule.RuleExpression = dto.RuleExpression;
        if (dto.BaseAmount != null)
            rule.BaseAmount = dto.BaseAmount;
        if (dto.EscalationRule != null)
            rule.EscalationRule = dto.EscalationRule;
        if (dto.CreditScoreAffected.HasValue)
            rule.CreditScoreAffected = dto.CreditScoreAffected.Value;
        if (dto.CreditScoreImpact.HasValue)
            rule.CreditScoreImpact = dto.CreditScoreImpact.Value;
        if (dto.PaymentRequired.HasValue)
            rule.PaymentRequired = dto.PaymentRequired.Value;
        if (dto.Description != null)
            rule.Description = dto.Description;
        if (dto.IsActive.HasValue)
            rule.IsActive = dto.IsActive.Value;

        rule.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.PenaltyRules.UpdateAsync(rule);
        await _unitOfWork.SaveChangesAsync();

        return await MapToRuleDto(rule);
    }

    public async Task DeletePenaltyRuleAsync(int userId, int id)
    {
        var rule = await _unitOfWork.PenaltyRules.GetByIdAsync(id);
        if (rule == null)
        {
            throw new BusinessException("惩罚规则不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, rule.ContractId, ContractOperation.DeleteCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        await _unitOfWork.PenaltyRules.DeleteAsync(rule);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResultDto<PenaltyExecutionDto>> GetExecutionRecordsAsync(QueryParameters parameters)
    {
        var pagedResult = await _unitOfWork.PenaltyExecutionRecords.GetPagedAsync(parameters);
        return await MapToPagedExecutionDto(pagedResult);
    }

    public async Task<PenaltyExecutionDto?> GetExecutionRecordByIdAsync(int id)
    {
        var record = await _unitOfWork.PenaltyExecutionRecords.GetByIdAsync(id);
        if (record == null) return null;
        return await MapToExecutionDto(record);
    }

    public async Task<List<PenaltyExecutionDto>> GetExecutionRecordsByContractIdAsync(int contractId)
    {
        var records = await _unitOfWork.PenaltyExecutionRecords.GetByContractIdAsync(contractId);
        var dtos = new List<PenaltyExecutionDto>();
        foreach (var record in records)
        {
            dtos.Add(await MapToExecutionDto(record));
        }
        return dtos;
    }

    public async Task<List<PenaltyExecutionDto>> GetExecutionRecordsByUserIdAsync(int userId)
    {
        var records = await _unitOfWork.PenaltyExecutionRecords.GetByUserIdAsync(userId);
        var dtos = new List<PenaltyExecutionDto>();
        foreach (var record in records)
        {
            dtos.Add(await MapToExecutionDto(record));
        }
        return dtos;
    }

    public async Task<List<PenaltyExecutionDto>> GetExecutionRecordsByContractIdAndUserIdAsync(int contractId, int userId)
    {
        var records = await _unitOfWork.PenaltyExecutionRecords.GetByContractIdAndUserIdAsync(contractId, userId);
        var dtos = new List<PenaltyExecutionDto>();
        foreach (var record in records)
        {
            dtos.Add(await MapToExecutionDto(record));
        }
        return dtos;
    }

    public async Task<PenaltyExecutionDto> CreateExecutionRecordAsync(int operatorId, PenaltyExecutionCreateDto dto)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(dto.ContractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(operatorId, dto.ContractId, ContractOperation.EditCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
        if (user == null)
        {
            throw new BusinessException("违约用户不存在", 404);
        }

        ContractViolation? violation = null;
        if (dto.ContractViolationId.HasValue)
        {
            violation = await _unitOfWork.ContractViolations.GetByIdAsync(dto.ContractViolationId.Value);
            if (violation == null)
            {
                throw new BusinessException("关联的违约记录不存在", 404);
            }
        }
        else
        {
            var allViolations = await _unitOfWork.ContractViolations.GetAllAsync();
            violation = allViolations
                .Where(v => v.ContractId == dto.ContractId && v.UserId == dto.UserId)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefault();
        }

        violation ??= new ContractViolation
        {
            ContractId = dto.ContractId,
            UserId = dto.UserId,
            ViolationDate = DateTime.UtcNow,
            ViolationType = ViolationType.Other,
            Reason = "系统自动生成违约记录",
            IsConfirmed = true
        };

        var rule = await _unitOfWork.PenaltyRules.GetActiveByContractIdAsync(dto.ContractId);

        var allRecords = await _unitOfWork.PenaltyExecutionRecords.GetByContractIdAndUserIdAsync(dto.ContractId, dto.UserId);
        var priorCount = allRecords.Count;

        var calcResult = _penaltyRuleParser.ParseAndCalculate(rule, contract, user, violation, priorCount);

        var record = new PenaltyExecutionRecord
        {
            PenaltyRuleId = rule?.Id ?? 0,
            ContractId = dto.ContractId,
            UserId = dto.UserId,
            ContractViolationId = violation.Id > 0 ? violation.Id : null,
            PenaltyType = calcResult.PenaltyType,
            Severity = calcResult.Severity,
            Status = PenaltyExecutionStatus.Pending,
            CalculatedContent = calcResult.CalculatedContent,
            Details = calcResult.Message,
            FinancialAmount = calcResult.FinancialAmount,
            CreditScoreChange = calcResult.CreditScoreChange,
            PaymentCompleted = false,
            ExecutionDeadline = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        if (calcResult.CreditScoreChange > 0)
        {
            user.CreditScore = Math.Max(0, user.CreditScore - calcResult.CreditScoreChange);
            await _unitOfWork.Users.UpdateAsync(user);
        }

        if (calcResult.FinancialAmount.HasValue && calcResult.FinancialAmount.Value > 0)
        {
            user.OutstandingPenaltyBalance += calcResult.FinancialAmount.Value;
            await _unitOfWork.Users.UpdateAsync(user);
        }

        var created = await _unitOfWork.PenaltyExecutionRecords.AddAsync(record);
        await _unitOfWork.SaveChangesAsync();

        return await MapToExecutionDto(created);
    }

    public async Task<PenaltyExecutionDto> UpdateExecutionRecordAsync(int operatorId, int id, PenaltyExecutionUpdateDto dto)
    {
        var record = await _unitOfWork.PenaltyExecutionRecords.GetByIdAsync(id);
        if (record == null)
        {
            throw new BusinessException("惩罚执行记录不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(operatorId, record.ContractId, ContractOperation.EditCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        var previousStatus = record.Status;
        var previousPaymentCompleted = record.PaymentCompleted;

        if (dto.Status.HasValue)
            record.Status = dto.Status.Value;
        if (dto.Details != null)
            record.Details = dto.Details;
        if (dto.PaymentCompleted.HasValue)
            record.PaymentCompleted = dto.PaymentCompleted.Value;
        if (dto.PaymentDate.HasValue)
            record.PaymentDate = dto.PaymentDate.Value;
        if (dto.CompletedAt.HasValue)
            record.CompletedAt = dto.CompletedAt.Value;

        if (dto.Status == PenaltyExecutionStatus.Completed && !dto.CompletedAt.HasValue)
        {
            record.CompletedAt = DateTime.UtcNow;
        }

        if (dto.PaymentCompleted == true && !previousPaymentCompleted && record.FinancialAmount.HasValue)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(record.UserId);
            if (user != null)
            {
                user.OutstandingPenaltyBalance = Math.Max(0, user.OutstandingPenaltyBalance - record.FinancialAmount.Value);
                await _unitOfWork.Users.UpdateAsync(user);
            }
            if (!dto.PaymentDate.HasValue)
            {
                record.PaymentDate = DateTime.UtcNow;
            }
        }

        record.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.PenaltyExecutionRecords.UpdateAsync(record);
        await _unitOfWork.SaveChangesAsync();

        return await MapToExecutionDto(record);
    }

    public async Task<PenaltyExecutionDto> WaiveExecutionRecordAsync(int operatorId, int id, PenaltyExecutionWaiveDto dto)
    {
        var record = await _unitOfWork.PenaltyExecutionRecords.GetByIdAsync(id);
        if (record == null)
        {
            throw new BusinessException("惩罚执行记录不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(operatorId, record.ContractId, ContractOperation.DeleteCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        if (record.CreditScoreChange > 0)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(record.UserId);
            if (user != null)
            {
                user.CreditScore = Math.Min(100, user.CreditScore + record.CreditScoreChange);
                await _unitOfWork.Users.UpdateAsync(user);
            }
        }

        if (record.FinancialAmount.HasValue && record.FinancialAmount.Value > 0 && !record.PaymentCompleted)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(record.UserId);
            if (user != null)
            {
                user.OutstandingPenaltyBalance = Math.Max(0, user.OutstandingPenaltyBalance - record.FinancialAmount.Value);
                await _unitOfWork.Users.UpdateAsync(user);
            }
        }

        record.Status = PenaltyExecutionStatus.Waived;
        record.WaivedByUserId = operatorId;
        record.WaivedReason = dto.WaivedReason;
        record.WaivedAt = DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.PenaltyExecutionRecords.UpdateAsync(record);
        await _unitOfWork.SaveChangesAsync();

        return await MapToExecutionDto(record);
    }

    public async Task<PenaltyOverviewDto> GetOverviewAsync()
    {
        var allRecords = await _unitOfWork.PenaltyExecutionRecords.GetAllAsync();
        var records = allRecords.ToList();

        return new PenaltyOverviewDto
        {
            TotalRecords = records.Count,
            PendingCount = records.Count(r => r.Status == PenaltyExecutionStatus.Pending),
            InProgressCount = records.Count(r => r.Status == PenaltyExecutionStatus.InProgress),
            CompletedCount = records.Count(r => r.Status == PenaltyExecutionStatus.Completed),
            WaivedCount = records.Count(r => r.Status == PenaltyExecutionStatus.Waived),
            FailedCount = records.Count(r => r.Status == PenaltyExecutionStatus.Failed),
            TotalFinancialAmount = records.Where(r => r.FinancialAmount.HasValue).Sum(r => r.FinancialAmount!.Value),
            PaidFinancialAmount = records.Where(r => r.FinancialAmount.HasValue && r.PaymentCompleted).Sum(r => r.FinancialAmount!.Value),
            UnpaidFinancialAmount = records.Where(r => r.FinancialAmount.HasValue && !r.PaymentCompleted).Sum(r => r.FinancialAmount!.Value),
            TotalCreditScoreImpact = records.Sum(r => r.CreditScoreChange)
        };
    }

    public async Task<List<PenaltyTrendDto>> GetTrendAsync(DateTime? startDate, DateTime? endDate)
    {
        var end = endDate ?? DateTime.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        if (start >= end)
        {
            throw new BusinessException("开始日期必须早于结束日期");
        }

        var allRecords = await _unitOfWork.PenaltyExecutionRecords.GetTrendByDateRangeAsync(start, end);
        var recordsInRange = allRecords
            .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
            .ToList();

        var trends = new List<PenaltyTrendDto>();
        for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            var nextDay = date.AddDays(1);
            var dayRecords = recordsInRange.Where(r => r.CreatedAt >= date && r.CreatedAt < nextDay).ToList();

            trends.Add(new PenaltyTrendDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                TotalCount = dayRecords.Count,
                CompletedCount = dayRecords.Count(r => r.Status == PenaltyExecutionStatus.Completed),
                PendingCount = dayRecords.Count(r => r.Status == PenaltyExecutionStatus.Pending),
                WaivedCount = dayRecords.Count(r => r.Status == PenaltyExecutionStatus.Waived),
                TotalFinancialAmount = dayRecords.Where(r => r.FinancialAmount.HasValue).Sum(r => r.FinancialAmount!.Value)
            });
        }

        return trends;
    }

    public async Task<DefaultPenaltyConfigDto> GetDefaultPenaltyConfigAsync(int contractId)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        var existingRule = await _unitOfWork.PenaltyRules.GetActiveByContractIdAsync(contractId);

        if (existingRule != null)
        {
            return new DefaultPenaltyConfigDto
            {
                ContractId = contractId,
                ContractName = contract.HabitName,
                Message = "该契约已配置惩罚规则。",
                RequiresConfiguration = false,
                SuggestedRule = await MapToRuleDto(existingRule)
            };
        }

        var suggestedRule = new PenaltyRuleDto
        {
            ContractId = contractId,
            ContractName = contract.HabitName,
            PenaltyType = PenaltyType.Custom,
            PenaltyTypeName = PenaltyTypeMigrator.GetPenaltyTypeName(PenaltyType.Custom),
            DefaultSeverity = PenaltySeverity.Medium,
            DefaultSeverityName = PenaltyTypeMigrator.GetSeverityName(PenaltySeverity.Medium),
            RuleExpression = "未完成打卡/达标需按约定履行惩罚",
            Description = _penaltyRuleParser.GenerateDefaultDescription(PenaltyType.Custom, PenaltySeverity.Medium),
            CreditScoreAffected = true,
            CreditScoreImpact = 5,
            PaymentRequired = false,
            IsActive = true
        };

        return new DefaultPenaltyConfigDto
        {
            ContractId = contractId,
            ContractName = contract.HabitName,
            Message = "该契约尚未配置惩罚规则，当前使用系统默认提示。建议管理员补充配置具体的惩罚内容、金额、信用分影响等规则。",
            RequiresConfiguration = true,
            SuggestedRule = suggestedRule
        };
    }

    public async Task<PenaltyRuleDto> SupplementPenaltyRuleAsync(int adminUserId, int contractId, PenaltyRuleCreateDto dto)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(adminUserId, contractId, ContractOperation.EditCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        var existingRule = await _unitOfWork.PenaltyRules.GetActiveByContractIdAsync(contractId);
        if (existingRule != null)
        {
            existingRule.IsActive = false;
            existingRule.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.PenaltyRules.UpdateAsync(existingRule);
        }

        dto.ContractId = contractId;
        return await CreatePenaltyRuleAsync(adminUserId, dto);
    }

    private async Task<PenaltyRuleDto> MapToRuleDto(PenaltyRule rule)
    {
        var dto = _mapper.Map<PenaltyRuleDto>(rule);
        dto.PenaltyTypeName = PenaltyTypeMigrator.GetPenaltyTypeName(rule.PenaltyType);
        dto.DefaultSeverityName = PenaltyTypeMigrator.GetSeverityName(rule.DefaultSeverity);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(rule.ContractId);
        dto.ContractName = contract?.HabitName;

        return dto;
    }

    private async Task<PagedResultDto<PenaltyRuleDto>> MapToPagedRuleDto(PagedResult<PenaltyRule> pagedResult)
    {
        var items = new List<PenaltyRuleDto>();
        foreach (var rule in pagedResult.Items)
        {
            items.Add(await MapToRuleDto(rule));
        }

        return new PagedResultDto<PenaltyRuleDto>
        {
            Items = items,
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalPages = pagedResult.TotalPages,
            HasPreviousPage = pagedResult.PageNumber > 1,
            HasNextPage = pagedResult.PageNumber < pagedResult.TotalPages
        };
    }

    private async Task<PenaltyExecutionDto> MapToExecutionDto(PenaltyExecutionRecord record)
    {
        var dto = _mapper.Map<PenaltyExecutionDto>(record);
        dto.PenaltyTypeName = PenaltyTypeMigrator.GetPenaltyTypeName(record.PenaltyType);
        dto.SeverityName = PenaltyTypeMigrator.GetSeverityName(record.Severity);
        dto.StatusName = PenaltyTypeMigrator.GetExecutionStatusName(record.Status);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(record.ContractId);
        dto.ContractName = contract?.HabitName;

        var user = await _unitOfWork.Users.GetByIdAsync(record.UserId);
        dto.Username = user?.Username;

        if (record.WaivedByUserId.HasValue)
        {
            var waivedBy = await _unitOfWork.Users.GetByIdAsync(record.WaivedByUserId.Value);
            dto.WaivedByUsername = waivedBy?.Username;
        }

        return dto;
    }

    private async Task<PagedResultDto<PenaltyExecutionDto>> MapToPagedExecutionDto(PagedResult<PenaltyExecutionRecord> pagedResult)
    {
        var items = new List<PenaltyExecutionDto>();
        foreach (var record in pagedResult.Items)
        {
            items.Add(await MapToExecutionDto(record));
        }

        return new PagedResultDto<PenaltyExecutionDto>
        {
            Items = items,
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalPages = pagedResult.TotalPages,
            HasPreviousPage = pagedResult.PageNumber > 1,
            HasNextPage = pagedResult.PageNumber < pagedResult.TotalPages
        };
    }
}
