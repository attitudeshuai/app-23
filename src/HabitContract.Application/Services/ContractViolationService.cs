using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class ContractViolationService : IContractViolationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ContractViolationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ContractViolationDto>> GetViolationsAsync(QueryParameters parameters)
    {
        var pagedResult = await _unitOfWork.ContractViolations.GetPagedAsync(parameters);
        return await MapToPagedViolationDto(pagedResult);
    }

    public async Task<ContractViolationDto> GetViolationByIdAsync(int id)
    {
        var violation = await _unitOfWork.ContractViolations.GetByIdAsync(id);
        if (violation == null)
        {
            throw new BusinessException("违约记录不存在", 404);
        }

        return await MapToViolationDto(violation);
    }

    public async Task<ContractViolationDto> CreateViolationAsync(int userId, ContractViolationCreateDto dto)
    {
        // 验证契约是否存在
        var contract = await _unitOfWork.Contracts.GetByIdAsync(dto.ContractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在");
        }

        var violation = _mapper.Map<ContractViolation>(dto);

        var created = await _unitOfWork.ContractViolations.AddAsync(violation);
        await _unitOfWork.SaveChangesAsync();

        return await MapToViolationDto(created);
    }

    public async Task<ContractViolationDto> UpdateViolationAsync(int userId, int id, ContractViolationUpdateDto dto)
    {
        var violation = await _unitOfWork.ContractViolations.GetByIdAsync(id);
        if (violation == null)
        {
            throw new BusinessException("违约记录不存在", 404);
        }

        // 权限校验：只有契约拥有者可以修改违约记录
        var contract = await _unitOfWork.Contracts.GetByIdAsync(violation.ContractId);
        if (contract == null || contract.OwnerId != userId)
        {
            throw new BusinessException("无权限修改此违约记录", 403);
        }

        if (dto.Reason != null)
            violation.Reason = dto.Reason;

        if (dto.IsConfirmed.HasValue)
            violation.IsConfirmed = dto.IsConfirmed.Value;

        violation.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.ContractViolations.UpdateAsync(violation);
        await _unitOfWork.SaveChangesAsync();

        return await MapToViolationDto(violation);
    }

    public async Task DeleteViolationAsync(int userId, int id)
    {
        var violation = await _unitOfWork.ContractViolations.GetByIdAsync(id);
        if (violation == null)
        {
            throw new BusinessException("违约记录不存在", 404);
        }

        // 权限校验：只有契约拥有者可以删除违约记录
        var contract = await _unitOfWork.Contracts.GetByIdAsync(violation.ContractId);
        if (contract == null || contract.OwnerId != userId)
        {
            throw new BusinessException("无权限删除此违约记录", 403);
        }

        await _unitOfWork.ContractViolations.DeleteAsync(violation);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// 将ContractViolation映射为DTO（包含契约名称）
    /// </summary>
    private async Task<ContractViolationDto> MapToViolationDto(ContractViolation violation)
    {
        var dto = _mapper.Map<ContractViolationDto>(violation);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(violation.ContractId);
        dto.ContractName = contract?.HabitName;

        return dto;
    }

    /// <summary>
    /// 将分页结果映射为ContractViolationDto分页
    /// </summary>
    private async Task<PagedResultDto<ContractViolationDto>> MapToPagedViolationDto(PagedResult<ContractViolation> pagedResult)
    {
        var items = new List<ContractViolationDto>();
        foreach (var violation in pagedResult.Items)
        {
            var dto = await MapToViolationDto(violation);
            items.Add(dto);
        }

        return new PagedResultDto<ContractViolationDto>
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
