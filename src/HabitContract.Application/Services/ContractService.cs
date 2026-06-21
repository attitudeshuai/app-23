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

    public ContractService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ContractListDto>> GetContractsAsync(QueryParameters parameters)
    {
        var pagedResult = await _unitOfWork.Contracts.GetPagedAsync(parameters);
        return await MapToPagedContractListDto(pagedResult);
    }

    public async Task<ContractDto> GetContractByIdAsync(int id)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        return await MapToContractDto(contract);
    }

    public async Task<ContractDto> CreateContractAsync(int userId, ContractCreateDto dto)
    {
        // 验证结束日期必须大于开始日期
        if (dto.EndDate <= dto.StartDate)
        {
            throw new BusinessException("结束日期必须大于开始日期");
        }

        var contract = _mapper.Map<Contract>(dto);
        contract.OwnerId = userId;
        contract.Status = ContractStatus.Active;

        var created = await _unitOfWork.Contracts.AddAsync(contract);
        await _unitOfWork.SaveChangesAsync();

        return await MapToContractDto(created);
    }

    public async Task<ContractDto> UpdateContractAsync(int userId, int id, ContractUpdateDto dto)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        // 只有契约拥有者可以修改
        if (contract.OwnerId != userId)
        {
            throw new BusinessException("无权限修改此契约", 403);
        }

        if (!string.IsNullOrEmpty(dto.HabitName))
            contract.HabitName = dto.HabitName;

        if (!string.IsNullOrEmpty(dto.Frequency))
            contract.Frequency = dto.Frequency;

        if (dto.StartDate.HasValue)
            contract.StartDate = dto.StartDate.Value;

        if (dto.EndDate.HasValue)
            contract.EndDate = dto.EndDate.Value;

        if (dto.PenaltyDescription != null)
            contract.PenaltyDescription = dto.PenaltyDescription;

        contract.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Contracts.UpdateAsync(contract);
        await _unitOfWork.SaveChangesAsync();

        return await MapToContractDto(contract);
    }

    public async Task DeleteContractAsync(int userId, int id)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        // 只有契约拥有者可以删除
        if (contract.OwnerId != userId)
        {
            throw new BusinessException("无权限删除此契约", 403);
        }

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

        // 只有契约拥有者可以修改状态
        if (contract.OwnerId != userId)
        {
            throw new BusinessException("无权限修改此契约状态", 403);
        }

        // 状态转换验证：只允许合法的状态流转
        var (isValid, errorMsg) = ValidateStatusTransition(contract.Status, dto.Status);
        if (!isValid)
        {
            throw new BusinessException(errorMsg);
        }

        contract.Status = dto.Status;
        contract.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Contracts.UpdateAsync(contract);
        await _unitOfWork.SaveChangesAsync();

        return await MapToContractDto(contract);
    }

    public async Task<PagedResultDto<ContractListDto>> GetMyContractsAsync(int userId, QueryParameters parameters)
    {
        // 获取用户作为拥有者的契约
        var allContracts = await _unitOfWork.Contracts.GetAllAsync();
        var ownerContractIds = allContracts.Where(c => c.OwnerId == userId).Select(c => c.Id).ToList();

        // 获取用户作为伙伴的契约
        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        var partnerContractIds = allPartners
            .Where(p => p.PartnerId == userId && p.Status == PartnerStatus.Accepted)
            .Select(p => p.ContractId)
            .ToList();

        var myContractIds = ownerContractIds.Union(partnerContractIds).Distinct().ToList();
        var myContracts = allContracts.Where(c => myContractIds.Contains(c.Id)).ToList();

        // 手动分页
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

    /// <summary>
    /// 验证契约状态转换是否合法
    /// 允许：Active->Paused, Paused->Active, Active->Completed, Active->Failed
    /// </summary>
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

    /// <summary>
    /// 将Contract实体映射为ContractDto（包含计算字段）
    /// </summary>
    private async Task<ContractDto> MapToContractDto(Contract contract)
    {
        var dto = _mapper.Map<ContractDto>(contract);

        // 获取拥有者名称
        var owner = await _unitOfWork.Users.GetByIdAsync(contract.OwnerId);
        dto.OwnerName = owner?.Username;

        // 计算伙伴数量
        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        dto.PartnerCount = allPartners.Count(p => p.ContractId == contract.Id);

        // 计算打卡数量
        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        dto.CheckInCount = allCheckIns.Count(c => c.ContractId == contract.Id);

        // 计算违约数量
        var allViolations = await _unitOfWork.ContractViolations.GetAllAsync();
        dto.ViolationCount = allViolations.Count(v => v.ContractId == contract.Id);

        return dto;
    }

    /// <summary>
    /// 将Contract实体映射为ContractListDto（包含拥有者名称）
    /// </summary>
    private async Task<ContractListDto> MapToContractListDto(Contract contract)
    {
        var dto = _mapper.Map<ContractListDto>(contract);

        var owner = await _unitOfWork.Users.GetByIdAsync(contract.OwnerId);
        dto.OwnerName = owner?.Username;

        return dto;
    }

    /// <summary>
    /// 将分页结果映射为ContractListDto分页
    /// </summary>
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
