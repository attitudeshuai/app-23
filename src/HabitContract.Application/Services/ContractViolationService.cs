using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class ContractViolationService : IContractViolationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEnumerable<INotificationSender> _notificationSenders;
    private readonly IPermissionService _permissionService;

    public ContractViolationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEnumerable<INotificationSender> notificationSenders,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationSenders = notificationSenders;
        _permissionService = permissionService;
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
        var contract = await _unitOfWork.Contracts.GetByIdAsync(dto.ContractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在");
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, dto.ContractId, ContractOperation.EditCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
        if (user == null)
        {
            throw new BusinessException("违约用户不存在");
        }

        var violation = _mapper.Map<ContractViolation>(dto);
        violation.UserId = dto.UserId;
        violation.ViolationType = ViolationTypeMigrator.Normalize(dto.ViolationType);
        violation.IsConfirmed = false;
        violation.CreatedAt = DateTime.UtcNow;

        var created = await _unitOfWork.ContractViolations.AddAsync(violation);
        await _unitOfWork.SaveChangesAsync();

        if (violation.IsSevere)
        {
            await NotifySevereViolationAsync(contract, violation);
        }

        return await MapToViolationDto(created);
    }

    public async Task<ContractViolationDto> UpdateViolationAsync(int userId, int id, ContractViolationUpdateDto dto)
    {
        var violation = await _unitOfWork.ContractViolations.GetByIdAsync(id);
        if (violation == null)
        {
            throw new BusinessException("违约记录不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, violation.ContractId, ContractOperation.EditCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        bool wasSevere = violation.IsSevere;

        if (dto.ViolationType.HasValue)
            violation.ViolationType = ViolationTypeMigrator.Normalize(dto.ViolationType.Value);

        if (dto.IsSevere.HasValue)
            violation.IsSevere = dto.IsSevere.Value;

        if (dto.Reason != null)
            violation.Reason = dto.Reason;

        if (dto.IsConfirmed.HasValue)
            violation.IsConfirmed = dto.IsConfirmed.Value;

        violation.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.ContractViolations.UpdateAsync(violation);
        await _unitOfWork.SaveChangesAsync();

        if (!wasSevere && violation.IsSevere)
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(violation.ContractId);
            if (contract != null)
            {
                await NotifySevereViolationAsync(contract, violation);
            }
        }

        return await MapToViolationDto(violation);
    }

    public async Task DeleteViolationAsync(int userId, int id)
    {
        var violation = await _unitOfWork.ContractViolations.GetByIdAsync(id);
        if (violation == null)
        {
            throw new BusinessException("违约记录不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, violation.ContractId, ContractOperation.DeleteCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        await _unitOfWork.ContractViolations.DeleteAsync(violation);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task NotifySevereViolationAsync(Contract contract, ContractViolation violation)
    {
        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        var supervisorIds = allPartners
            .Where(p => p.ContractId == contract.Id && p.Role == PartnerRole.Supervisor && p.Status == PartnerStatus.Accepted && p.PartnerId != violation.UserId)
            .Select(p => p.PartnerId)
            .ToList();

        if (contract.OwnerId != violation.UserId && !supervisorIds.Contains(contract.OwnerId))
        {
            supervisorIds.Add(contract.OwnerId);
        }

        if (!supervisorIds.Any())
        {
            return;
        }

        var normalizedType = ViolationTypeMigrator.Normalize(violation.ViolationType);
        var allUsers = await _unitOfWork.Users.GetAllAsync();
        var violator = allUsers.FirstOrDefault(u => u.Id == violation.UserId);
        var violatorName = violator?.Username ?? "未知用户";
        var violationTypeName = GetViolationTypeName(normalizedType);

        foreach (var supervisorId in supervisorIds)
        {
            var supervisor = allUsers.FirstOrDefault(u => u.Id == supervisorId);
            if (supervisor == null) continue;

            var title = $"【严重违约提醒】契约「{contract.HabitName}」";
            var content = $"⚠️ 严重违约提醒：用户「{violatorName}」发生{violationTypeName}违约。\n日期：{violation.ViolationDate:yyyy-MM-dd}\n原因：{violation.Reason}\n请监督伙伴重点关注，及时跟进。";

            foreach (var sender in _notificationSenders)
            {
                try
                {
                    await sender.SendAsync(supervisor, title, content);
                }
                catch
                {
                }
            }
        }
    }

    private static string GetViolationTypeName(ViolationType type)
    {
        return type switch
        {
            ViolationType.MissedCheckIn => "未打卡",
            ViolationType.NotMetTarget => "未达标",
            ViolationType.Other => "其他",
            _ => "未知"
        };
    }

    private async Task<ContractViolationDto> MapToViolationDto(ContractViolation violation)
    {
        var dto = _mapper.Map<ContractViolationDto>(violation);
        dto.ViolationType = ViolationTypeMigrator.Normalize(violation.ViolationType);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(violation.ContractId);
        dto.ContractName = contract?.HabitName;

        var user = await _unitOfWork.Users.GetByIdAsync(violation.UserId);
        dto.Username = user?.Username;

        return dto;
    }

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
