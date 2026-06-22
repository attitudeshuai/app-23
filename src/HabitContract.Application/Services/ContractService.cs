using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class ContractService : IContractService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFrequencyRuleCache _frequencyCache;
    private readonly IFrequencyParser _frequencyParser;
    private readonly ICheckInService _checkInService;
    private readonly IPermissionService _permissionService;

    public ContractService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFrequencyRuleCache frequencyCache,
        IFrequencyParser frequencyParser,
        ICheckInService checkInService,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _frequencyCache = frequencyCache;
        _frequencyParser = frequencyParser;
        _checkInService = checkInService;
        _permissionService = permissionService;
    }

    public async Task<PagedResultDto<ContractListDto>> GetContractsAsync(QueryParameters parameters)
    {
        var pagedResult = await _unitOfWork.Contracts.GetPagedAsync(parameters);
        return await MapToPagedContractListDto(pagedResult);
    }

    public async Task<ContractDto> GetContractByIdAsync(int userId, int id)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, id, ContractOperation.ViewContract);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        return await MapToContractDto(contract);
    }

    public async Task<ContractDto> CreateContractAsync(int userId, ContractCreateDto dto)
    {
        if (dto.EndDate <= dto.StartDate)
        {
            throw new BusinessException("结束日期必须大于开始日期");
        }

        var contract = _mapper.Map<Contract>(dto);
        contract.OwnerId = userId;
        contract.Status = ContractStatus.Active;

        var created = await _unitOfWork.Contracts.AddAsync(contract);
        await _unitOfWork.SaveChangesAsync();

        var frequencyRule = _frequencyParser.Parse(contract.Frequency);
        await _frequencyCache.SetAsync(contract.Id, frequencyRule);

        return await MapToContractDto(created);
    }

    public async Task<ContractDto> UpdateContractAsync(int userId, int id, ContractUpdateDto dto)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, id, ContractOperation.EditContract);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        var frequencyChanged = false;
        FrequencyRule? newRule = null;

        if (!string.IsNullOrEmpty(dto.HabitName))
            contract.HabitName = dto.HabitName;

        if (!string.IsNullOrEmpty(dto.Frequency) && dto.Frequency != contract.Frequency)
        {
            contract.Frequency = dto.Frequency;
            frequencyChanged = true;
            newRule = _frequencyParser.Parse(dto.Frequency);
        }

        if (dto.StartDate.HasValue)
            contract.StartDate = dto.StartDate.Value;

        if (dto.EndDate.HasValue)
            contract.EndDate = dto.EndDate.Value;

        if (dto.PenaltyDescription != null)
            contract.PenaltyDescription = dto.PenaltyDescription;

        if (dto.CheckInDeadline.HasValue)
            contract.CheckInDeadline = dto.CheckInDeadline.Value;

        if (!string.IsNullOrEmpty(dto.TimeZone))
            contract.TimeZone = dto.TimeZone;

        if (dto.MakeUpDeadlineDays.HasValue)
            contract.MakeUpDeadlineDays = dto.MakeUpDeadlineDays.Value;

        contract.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Contracts.UpdateAsync(contract);
        await _unitOfWork.SaveChangesAsync();

        if (frequencyChanged && newRule != null)
        {
            await _frequencyCache.RemoveAsync(contract.Id);
            await _frequencyCache.SetAsync(contract.Id, newRule);
            await _checkInService.ReValidateRecentCheckInsAsync(contract.Id, newRule);
        }

        return await MapToContractDto(contract);
    }

    public async Task DeleteContractAsync(int userId, int id)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, id, ContractOperation.DeleteContract);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        await _frequencyCache.RemoveAsync(contract.Id);
        await _unitOfWork.Contracts.DeleteAsync(contract);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ContractDto> UpdateContractStatusAsync(int userId, int id, ContractStatusDto dto)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, id, ContractOperation.ChangeContractStatus);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        var (isValid, errorMsg2) = ValidateStatusTransition(contract.Status, dto.Status);
        if (!isValid)
        {
            throw new BusinessException(errorMsg2);
        }

        var previousStatus = contract.Status;
        contract.Status = dto.Status;
        contract.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Contracts.UpdateAsync(contract);

        if (dto.Status == ContractStatus.Completed && previousStatus != ContractStatus.Completed && contract.TemplateId.HasValue)
        {
            var template = await _unitOfWork.HabitTemplates.GetByIdAsync(contract.TemplateId.Value);
            if (template != null)
            {
                template.CompletionCount++;
                template.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.HabitTemplates.UpdateAsync(template);
            }
        }

        if (dto.Status != ContractStatus.Active)
        {
            await _frequencyCache.RemoveAsync(contract.Id);
        }

        await _unitOfWork.SaveChangesAsync();

        return await MapToContractDto(contract);
    }

    public async Task<PagedResultDto<ContractListDto>> GetMyContractsAsync(int userId, QueryParameters parameters)
    {
        var allContracts = await _unitOfWork.Contracts.GetAllAsync();
        var ownerContractIds = allContracts.Where(c => c.OwnerId == userId).Select(c => c.Id).ToList();

        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        var partnerContractIds = allPartners
            .Where(p => p.PartnerId == userId && p.Status == PartnerStatus.Accepted)
            .Select(p => p.ContractId)
            .ToList();

        var myContractIds = ownerContractIds.Union(partnerContractIds).Distinct().ToList();
        var myContracts = allContracts.Where(c => myContractIds.Contains(c.Id)).ToList();

        var totalCount = myContracts.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
        var pagedItems = myContracts
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var items = new List<ContractListDto>();
        foreach (var contract in pagedItems)
        {
            var dto = await MapToContractListDto(contract);
            items.Add(dto);
        }

        return new PagedResultDto<ContractListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = parameters.PageNumber > 1,
            HasNextPage = parameters.PageNumber < totalPages
        };
    }

    private static (bool IsValid, string ErrorMsg) ValidateStatusTransition(ContractStatus current, ContractStatus target)
    {
        return (current, target) switch
        {
            (ContractStatus.Active, ContractStatus.Paused) => (true, string.Empty),
            (ContractStatus.Paused, ContractStatus.Active) => (true, string.Empty),
            (ContractStatus.Active, ContractStatus.Completed) => (true, string.Empty),
            (ContractStatus.Active, ContractStatus.Failed) => (true, string.Empty),
            _ => (false, $"不允许从 {current} 状态转换到 {target} 状态")
        };
    }

    private async Task<ContractDto> MapToContractDto(Contract contract)
    {
        var dto = _mapper.Map<ContractDto>(contract);

        var owner = await _unitOfWork.Users.GetByIdAsync(contract.OwnerId);
        dto.OwnerName = owner?.Username;

        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        dto.PartnerCount = allPartners.Count(p => p.ContractId == contract.Id);

        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        dto.CheckInCount = allCheckIns.Count(c => c.ContractId == contract.Id);

        var allViolations = await _unitOfWork.ContractViolations.GetAllAsync();
        dto.ViolationCount = allViolations.Count(v => v.ContractId == contract.Id);

        var streaks = await _checkInService.GetStreaksAsync(contract.Id, contract.OwnerId);
        dto.CurrentStreak = streaks.CurrentStreak;
        dto.LongestStreak = streaks.LongestStreak;

        return dto;
    }

    private async Task<ContractListDto> MapToContractListDto(Contract contract)
    {
        var dto = _mapper.Map<ContractListDto>(contract);

        var owner = await _unitOfWork.Users.GetByIdAsync(contract.OwnerId);
        dto.OwnerName = owner?.Username;

        var streaks = await _checkInService.GetStreaksAsync(contract.Id, contract.OwnerId);
        dto.CurrentStreak = streaks.CurrentStreak;

        return dto;
    }

    private async Task<PagedResultDto<ContractListDto>> MapToPagedContractListDto(PagedResult<Contract> pagedResult)
    {
        var items = new List<ContractListDto>();
        foreach (var contract in pagedResult.Items)
        {
            var dto = await MapToContractListDto(contract);
            items.Add(dto);
        }

        return new PagedResultDto<ContractListDto>
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
