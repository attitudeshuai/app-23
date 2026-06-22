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
    private readonly IPermissionService _permissionService;
    private readonly IRoleChangeAuditService _roleChangeAuditService;
    private readonly IEnumerable<INotificationSender> _notificationSenders;

    public ContractPartnerService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPermissionService permissionService,
        IRoleChangeAuditService roleChangeAuditService,
        IEnumerable<INotificationSender> notificationSenders)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _permissionService = permissionService;
        _roleChangeAuditService = roleChangeAuditService;
        _notificationSenders = notificationSenders;
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
        var contract = await _unitOfWork.Contracts.GetByIdAsync(dto.ContractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在");
        }

        if (contract.OwnerId != userId)
        {
            throw new BusinessException("无权限为此契约添加监督伙伴", 403);
        }

        var partnerUser = await _unitOfWork.Users.GetByIdAsync(dto.PartnerId);
        if (partnerUser == null)
        {
            throw new BusinessException("监督用户不存在");
        }

        if (dto.PartnerId == userId)
        {
            throw new BusinessException("不能将自己设为监督伙伴");
        }

        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        if (allPartners.Any(p => p.ContractId == dto.ContractId && p.PartnerId == dto.PartnerId))
        {
            throw new BusinessException("该用户已是此契约的监督伙伴");
        }

        var partner = new ContractPartner
        {
            ContractId = dto.ContractId,
            PartnerId = dto.PartnerId,
            Role = dto.Role,
            Status = PartnerStatus.Pending
        };

        var created = await _unitOfWork.ContractPartners.AddAsync(partner);
        await _unitOfWork.SaveChangesAsync();

        await NotifyPartnerInvitedAsync(contract, partnerUser, dto.Role);

        return await MapToPartnerDto(created);
    }

    public async Task<ContractPartnerDto> UpdatePartnerAsync(int userId, int id, ContractPartnerUpdateDto dto)
    {
        var partner = await _unitOfWork.ContractPartners.GetByIdAsync(id);
        if (partner == null)
        {
            throw new BusinessException("监督伙伴不存在", 404);
        }

        var contract = await _unitOfWork.Contracts.GetByIdAsync(partner.ContractId);
        if (contract == null)
        {
            throw new BusinessException("关联契约不存在");
        }

        var isOwner = contract.OwnerId == userId;
        var isPartnerSelf = partner.PartnerId == userId;

        if (!isOwner && !isPartnerSelf)
        {
            throw new BusinessException("无权限修改此监督伙伴信息", 403);
        }

        var oldRole = partner.Role;
        var roleChanged = false;

        if (dto.Role.HasValue && isOwner)
        {
            if (dto.Role.Value != partner.Role)
            {
                oldRole = partner.Role;
                partner.Role = dto.Role.Value;
                roleChanged = true;
            }
        }
        else if (dto.Role.HasValue && !isOwner)
        {
            throw new BusinessException("只有契约拥有者可以修改伙伴角色", 403);
        }

        if (dto.Status.HasValue)
        {
            if (isPartnerSelf && partner.Status == PartnerStatus.Pending)
            {
                if (dto.Status.Value != PartnerStatus.Accepted && dto.Status.Value != PartnerStatus.Rejected)
                {
                    throw new BusinessException("只能接受或拒绝待处理的邀请");
                }
                partner.Status = dto.Status.Value;
                if (dto.Status.Value == PartnerStatus.Accepted && !partner.JoinedAt.HasValue)
                {
                    partner.JoinedAt = DateTime.UtcNow;
                }
            }
            else if (isOwner)
            {
                partner.Status = dto.Status.Value;
            }
            else
            {
                throw new BusinessException("无权限修改伙伴状态", 403);
            }
        }

        partner.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.ContractPartners.UpdateAsync(partner);
        await _unitOfWork.SaveChangesAsync();

        if (roleChanged)
        {
            await _roleChangeAuditService.CreateAuditRecordAsync(
                partner.ContractId,
                partner.PartnerId,
                oldRole,
                partner.Role,
                userId,
                "契约拥有者修改伙伴角色");

            await NotifyRoleChangedAsync(contract, partner, oldRole, userId);
        }

        return await MapToPartnerDto(partner);
    }

    public async Task DeletePartnerAsync(int userId, int id)
    {
        var partner = await _unitOfWork.ContractPartners.GetByIdAsync(id);
        if (partner == null)
        {
            throw new BusinessException("监督伙伴不存在", 404);
        }

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

        if (partner.PartnerId != userId)
        {
            throw new BusinessException("无权限修改此伙伴状态", 403);
        }

        if (partner.Status != PartnerStatus.Pending)
        {
            throw new BusinessException("只能响应待处理的邀请");
        }

        if (dto.Status != PartnerStatus.Accepted && dto.Status != PartnerStatus.Rejected)
        {
            throw new BusinessException("只能接受或拒绝待处理的邀请");
        }

        partner.Status = dto.Status;

        if (dto.Status == PartnerStatus.Accepted)
        {
            partner.JoinedAt = DateTime.UtcNow;
        }

        partner.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.ContractPartners.UpdateAsync(partner);
        await _unitOfWork.SaveChangesAsync();

        var contract = await _unitOfWork.Contracts.GetByIdAsync(partner.ContractId);
        if (contract != null)
        {
            await NotifyPartnerStatusChangedAsync(contract, partner, userId);
        }

        return await MapToPartnerDto(partner);
    }

    private async Task NotifyPartnerInvitedAsync(Contract contract, User partnerUser, PartnerRole role)
    {
        var roleName = GetRoleDisplayName(role);
        var title = $"您被邀请成为契约「{contract.HabitName}」的{roleName}";
        var content = $"契约发起者邀请您担任{roleName}，请前往应用查看并接受邀请。";

        foreach (var sender in _notificationSenders)
        {
            try
            {
                await sender.SendAsync(partnerUser, title, content);
            }
            catch
            {
            }
        }
    }

    private async Task NotifyRoleChangedAsync(Contract contract, ContractPartner partner, PartnerRole oldRole, int changedByUserId)
    {
        var partnerUser = await _unitOfWork.Users.GetByIdAsync(partner.PartnerId);
        var changedByUser = await _unitOfWork.Users.GetByIdAsync(changedByUserId);
        if (partnerUser == null)
        {
            return;
        }

        var oldRoleName = GetRoleDisplayName(oldRole);
        var newRoleName = GetRoleDisplayName(partner.Role);
        var changedByName = changedByUser?.Username ?? "未知用户";

        var title = $"您在契约「{contract.HabitName}」中的角色已变更";
        var content = $"您的角色由「{oldRoleName}」变更为「{newRoleName}」，由「{changedByName}」操作。";

        foreach (var sender in _notificationSenders)
        {
            try
            {
                await sender.SendAsync(partnerUser, title, content);
            }
            catch
            {
            }
        }

        var owner = await _unitOfWork.Users.GetByIdAsync(contract.OwnerId);
        if (owner != null && owner.Id != changedByUserId)
        {
            var ownerTitle = $"契约「{contract.HabitName}」伙伴角色变更";
            var ownerContent = $"伙伴「{partnerUser.Username}」的角色由「{oldRoleName}」变更为「{newRoleName}」。";

            foreach (var sender in _notificationSenders)
            {
                try
                {
                    await sender.SendAsync(owner, ownerTitle, ownerContent);
                }
                catch
                {
                }
            }
        }
    }

    private async Task NotifyPartnerStatusChangedAsync(Contract contract, ContractPartner partner, int partnerUserId)
    {
        var owner = await _unitOfWork.Users.GetByIdAsync(contract.OwnerId);
        var partnerUser = await _unitOfWork.Users.GetByIdAsync(partnerUserId);
        if (owner == null || partnerUser == null)
        {
            return;
        }

        var statusText = partner.Status == PartnerStatus.Accepted ? "接受了" : "拒绝了";
        var roleName = GetRoleDisplayName(partner.Role);

        var title = $"契约「{contract.HabitName}」伙伴{statusText}邀请";
        var content = $"用户「{partnerUser.Username}」{statusText}成为{roleName}的邀请。";

        foreach (var sender in _notificationSenders)
        {
            try
            {
                await sender.SendAsync(owner, title, content);
            }
            catch
            {
            }
        }
    }

    private static string GetRoleDisplayName(PartnerRole role)
    {
        return role switch
        {
            PartnerRole.Supervisor => "监督人",
            PartnerRole.Witness => "见证人",
            PartnerRole.Supporter => "支持者",
            _ => role.ToString()
        };
    }

    private async Task<ContractPartnerDto> MapToPartnerDto(ContractPartner partner)
    {
        var dto = _mapper.Map<ContractPartnerDto>(partner);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(partner.ContractId);
        dto.ContractName = contract?.HabitName;

        var partnerUser = await _unitOfWork.Users.GetByIdAsync(partner.PartnerId);
        dto.PartnerName = partnerUser?.Username;

        return dto;
    }

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
