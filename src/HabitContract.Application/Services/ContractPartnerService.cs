using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class ContractPartnerService : IContractPartnerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ContractPartnerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ContractPartnerDto>> GetPartnersAsync(QueryParameters parameters)
    {
        var pagedResult = await _unitOfWork.ContractPartners.GetPagedAsync(parameters);
        return await MapToPagedPartnerDto(pagedResult);
    }

    public async Task<ContractPartnerDto> GetPartnerByIdAsync(int id)
    {
        var partner = await _unitOfWork.ContractPartners.GetByIdAsync(id);
        if (partner == null)
        {
            throw new BusinessException("监督伙伴不存在", 404);
        }

        return await MapToPartnerDto(partner);
    }

    public async Task<ContractPartnerDto> CreatePartnerAsync(int userId, ContractPartnerCreateDto dto)
    {
        // 验证契约是否存在
        var contract = await _unitOfWork.Contracts.GetByIdAsync(dto.ContractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在");
        }

        // 只有契约拥有者可以添加伙伴
        if (contract.OwnerId != userId)
        {
            throw new BusinessException("无权限为此契约添加监督伙伴");
        }

        // 验证伙伴用户是否存在
        var partnerUser = await _unitOfWork.Users.GetByIdAsync(dto.PartnerId);
        if (partnerUser == null)
        {
            throw new BusinessException("监督用户不存在");
        }

        // 不能将自己设为监督伙伴
        if (dto.PartnerId == userId)
        {
            throw new BusinessException("不能将自己设为监督伙伴");
        }

        // 检查是否已存在重复的伙伴关系
        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        if (allPartners.Any(p => p.ContractId == dto.ContractId && p.PartnerId == dto.PartnerId))
        {
            throw new BusinessException("该用户已是此契约的监督伙伴");
        }

        var partner = new ContractPartner
        {
            ContractId = dto.ContractId,
            PartnerId = dto.PartnerId,
            Status = PartnerStatus.Pending
        };

        var created = await _unitOfWork.ContractPartners.AddAsync(partner);
        await _unitOfWork.SaveChangesAsync();

        return await MapToPartnerDto(created);
    }

    public async Task<ContractPartnerDto> UpdatePartnerAsync(int userId, int id, ContractPartnerUpdateDto dto)
    {
        var partner = await _unitOfWork.ContractPartners.GetByIdAsync(id);
        if (partner == null)
        {
            throw new BusinessException("监督伙伴不存在", 404);
        }

        // 权限校验：契约拥有者或伙伴本人可以修改
        var contract = await _unitOfWork.Contracts.GetByIdAsync(partner.ContractId);
        if (contract == null)
        {
            throw new BusinessException("关联契约不存在");
        }

        if (contract.OwnerId != userId && partner.PartnerId != userId)
        {
            throw new BusinessException("无权限修改此监督伙伴信息", 403);
        }

        if (dto.Status.HasValue)
            partner.Status = dto.Status.Value;

        if (dto.Status == PartnerStatus.Accepted && !partner.JoinedAt.HasValue)
        {
            partner.JoinedAt = DateTime.UtcNow;
        }

        await _unitOfWork.ContractPartners.UpdateAsync(partner);
        await _unitOfWork.SaveChangesAsync();

        return await MapToPartnerDto(partner);
    }

    public async Task DeletePartnerAsync(int userId, int id)
    {
        var partner = await _unitOfWork.ContractPartners.GetByIdAsync(id);
        if (partner == null)
        {
            throw new BusinessException("监督伙伴不存在", 404);
        }

        // 只有契约拥有者可以移除伙伴
        var contract = await _unitOfWork.Contracts.GetByIdAsync(partner.ContractId);
        if (contract == null || contract.OwnerId != userId)
        {
            throw new BusinessException("无权限移除此监督伙伴", 403);
        }

        await _unitOfWork.ContractPartners.DeleteAsync(partner);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ContractPartnerDto> UpdatePartnerStatusAsync(int userId, int id, ContractPartnerStatusDto dto)
    {
        var partner = await _unitOfWork.ContractPartners.GetByIdAsync(id);
        if (partner == null)
        {
            throw new BusinessException("监督伙伴不存在", 404);
        }

        // 只有伙伴本人可以响应邀请
        if (partner.PartnerId != userId)
        {
            throw new BusinessException("无权限修改此伙伴状态", 403);
        }

        // 状态转换验证：Pending->Accepted 或 Pending->Rejected
        if (partner.Status != PartnerStatus.Pending)
        {
            throw new BusinessException("只能响应待处理的邀请");
        }

        if (dto.Status != PartnerStatus.Accepted && dto.Status != PartnerStatus.Rejected)
        {
            throw new BusinessException("只能接受或拒绝待处理的邀请");
        }

        partner.Status = dto.Status;

        // 接受邀请时记录加入时间
        if (dto.Status == PartnerStatus.Accepted)
        {
            partner.JoinedAt = DateTime.UtcNow;
        }

        await _unitOfWork.ContractPartners.UpdateAsync(partner);
        await _unitOfWork.SaveChangesAsync();

        return await MapToPartnerDto(partner);
    }

    /// <summary>
    /// 将ContractPartner映射为DTO（包含关联名称）
    /// </summary>
    private async Task<ContractPartnerDto> MapToPartnerDto(ContractPartner partner)
    {
        var dto = _mapper.Map<ContractPartnerDto>(partner);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(partner.ContractId);
        dto.ContractName = contract?.HabitName;

        var partnerUser = await _unitOfWork.Users.GetByIdAsync(partner.PartnerId);
        dto.PartnerName = partnerUser?.Username;

        return dto;
    }

    /// <summary>
    /// 将分页结果映射为ContractPartnerDto分页
    /// </summary>
    private async Task<PagedResultDto<ContractPartnerDto>> MapToPagedPartnerDto(PagedResult<ContractPartner> pagedResult)
    {
        var items = new List<ContractPartnerDto>();
        foreach (var partner in pagedResult.Items)
        {
            var dto = await MapToPartnerDto(partner);
            items.Add(dto);
        }

        return new PagedResultDto<ContractPartnerDto>
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
