using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class MakeUpRequestService : IMakeUpRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;
    private readonly IEnumerable<INotificationSender> _notificationSenders;
    private readonly ICheckInService _checkInService;

    public MakeUpRequestService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPermissionService permissionService,
        IEnumerable<INotificationSender> notificationSenders,
        ICheckInService checkInService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _permissionService = permissionService;
        _notificationSenders = notificationSenders;
        _checkInService = checkInService;
    }

    public async Task<PagedResultDto<MakeUpRequestListDto>> GetMakeUpRequestsAsync(QueryParameters parameters)
    {
        var pagedResult = await _unitOfWork.MakeUpRequests.GetPagedAsync(parameters);
        return await MapToPagedMakeUpRequestListDto(pagedResult);
    }

    public async Task<MakeUpRequestDto> GetMakeUpRequestByIdAsync(int userId, int id)
    {
        var request = await _unitOfWork.MakeUpRequests.GetByIdAsync(id);
        if (request == null)
        {
            throw new BusinessException("补卡申请不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, request.ContractId, ContractOperation.ViewCheckIns);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        return await MapToMakeUpRequestDto(request);
    }

    public async Task<MakeUpRequestDto> CreateMakeUpRequestAsync(int userId, MakeUpRequestCreateDto dto)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(dto.ContractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在");
        }

        if (contract.Status != ContractStatus.Active)
        {
            throw new BusinessException("只能为进行中的契约申请补卡");
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, dto.ContractId, ContractOperation.CreateCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        var checkInDate = dto.CheckInDate.Date;

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(contract.TimeZone);
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        var makeUpDeadline = checkInDate.AddDays(contract.MakeUpDeadlineDays);

        if (nowLocal.Date > makeUpDeadline)
        {
            throw new BusinessException($"已超过补卡申请期限（补卡截止日期：{makeUpDeadline:yyyy-MM-dd}）");
        }

        if (checkInDate >= nowLocal.Date)
        {
            throw new BusinessException("只能申请补打过去日期的卡");
        }

        var existingCheckIn = await _unitOfWork.CheckIns.GetByDateAsync(dto.ContractId, userId, checkInDate);
        if (existingCheckIn != null && existingCheckIn.Status != CheckInStatus.Missed)
        {
            throw new BusinessException("该日期已有有效打卡记录");
        }

        var existingRequest = await _unitOfWork.MakeUpRequests.GetByDateAsync(dto.ContractId, userId, checkInDate);
        if (existingRequest != null && existingRequest.Status == MakeUpRequestStatus.Pending)
        {
            throw new BusinessException("该日期已有待审核的补卡申请");
        }

        var request = _mapper.Map<MakeUpRequest>(dto);
        request.UserId = userId;
        request.CheckInDate = checkInDate;

        var created = await _unitOfWork.MakeUpRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        await NotifySupervisorsForReview(contract, created);

        return await MapToMakeUpRequestDto(created);
    }

    public async Task<MakeUpRequestDto> ReviewMakeUpRequestAsync(int reviewerId, int id, MakeUpRequestReviewDto dto)
    {
        var request = await _unitOfWork.MakeUpRequests.GetByIdAsync(id);
        if (request == null)
        {
            throw new BusinessException("补卡申请不存在", 404);
        }

        if (request.Status != MakeUpRequestStatus.Pending)
        {
            throw new BusinessException("该申请已被审核，无法重复审核");
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(reviewerId, request.ContractId, ContractOperation.ReviewCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        var isSupervisor = allPartners.Any(p =>
            p.ContractId == request.ContractId &&
            p.PartnerId == reviewerId &&
            p.Role == PartnerRole.Supervisor &&
            p.Status == PartnerStatus.Accepted);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(request.ContractId);
        if (contract == null)
        {
            throw new BusinessException("关联契约不存在", 404);
        }

        if (!isSupervisor && contract.OwnerId != reviewerId)
        {
            throw new BusinessException("只有监督伙伴或契约创建者可以审核补卡申请", 403);
        }

        if (dto.Status == MakeUpRequestStatus.Rejected && string.IsNullOrWhiteSpace(dto.RejectionReason))
        {
            throw new BusinessException("拒绝申请时必须说明原因");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            request.Status = dto.Status;
            request.ReviewedBy = reviewerId;
            request.ReviewedAt = DateTime.UtcNow;
            request.RejectionReason = dto.Status == MakeUpRequestStatus.Rejected ? dto.RejectionReason : null;

            await _unitOfWork.MakeUpRequests.UpdateAsync(request);

            if (dto.Status == MakeUpRequestStatus.Approved)
            {
                var existingCheckIn = await _unitOfWork.CheckIns.GetByDateAsync(request.ContractId, request.UserId, request.CheckInDate);

                if (existingCheckIn != null)
                {
                    existingCheckIn.Status = CheckInStatus.MakeUp;
                    existingCheckIn.StatusChangedAt = DateTime.UtcNow;
                    existingCheckIn.MakeUpRequestId = request.Id;
                    existingCheckIn.ProofText = request.ProofText;
                    existingCheckIn.ProofPhoto = request.ProofPhoto;
                    existingCheckIn.ConsecutiveDays = await _unitOfWork.CheckIns.GetConsecutiveDaysAsync(
                        request.ContractId, request.UserId, request.CheckInDate);

                    await _unitOfWork.CheckIns.UpdateAsync(existingCheckIn);
                }
                else
                {
                    var checkIn = new CheckIn
                    {
                        ContractId = request.ContractId,
                        UserId = request.UserId,
                        CheckInDate = request.CheckInDate,
                        CheckInTime = DateTime.UtcNow,
                        ProofText = request.ProofText,
                        ProofPhoto = request.ProofPhoto,
                        Status = CheckInStatus.MakeUp,
                        StatusChangedAt = DateTime.UtcNow,
                        MakeUpRequestId = request.Id,
                        ConsecutiveDays = await _unitOfWork.CheckIns.GetConsecutiveDaysAsync(
                            request.ContractId, request.UserId, request.CheckInDate)
                    };

                    await _unitOfWork.CheckIns.AddAsync(checkIn);
                }

                await _unitOfWork.SaveChangesAsync();

                var allCheckIns = await _unitOfWork.CheckIns.GetByContractAndUserIdAsync(request.ContractId, request.UserId);
                var subsequentCheckIns = allCheckIns
                    .Where(ci => ci.CheckInDate > request.CheckInDate && ci.Status != CheckInStatus.Missed)
                    .OrderBy(ci => ci.CheckInDate)
                    .ToList();

                foreach (var checkIn in subsequentCheckIns)
                {
                    checkIn.ConsecutiveDays = await _unitOfWork.CheckIns.GetConsecutiveDaysAsync(
                        request.ContractId, request.UserId, checkIn.CheckInDate);
                    await _unitOfWork.CheckIns.UpdateAsync(checkIn);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        await NotifyApplicantReviewResult(request, contract);

        return await MapToMakeUpRequestDto(request);
    }

    public async Task<PagedResultDto<MakeUpRequestListDto>> GetMyMakeUpRequestsAsync(int userId, QueryParameters parameters)
    {
        var allRequests = await _unitOfWork.MakeUpRequests.GetAllAsync();
        var myRequests = allRequests.Where(r => r.UserId == userId).ToList();

        var totalCount = myRequests.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
        var pagedItems = myRequests
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var items = new List<MakeUpRequestListDto>();
        foreach (var request in pagedItems)
        {
            var dto = await MapToMakeUpRequestListDto(request);
            items.Add(dto);
        }

        return new PagedResultDto<MakeUpRequestListDto>
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

    public async Task<PagedResultDto<MakeUpRequestListDto>> GetPendingReviewsAsync(int reviewerId, QueryParameters parameters)
    {
        var allContracts = await _unitOfWork.Contracts.GetAllAsync();
        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();

        var supervisedContractIds = allPartners
            .Where(p => p.PartnerId == reviewerId && p.Role == PartnerRole.Supervisor && p.Status == PartnerStatus.Accepted)
            .Select(p => p.ContractId)
            .ToList();

        var ownedContractIds = allContracts
            .Where(c => c.OwnerId == reviewerId)
            .Select(c => c.Id)
            .ToList();

        var allReviewableContractIds = supervisedContractIds.Union(ownedContractIds).ToList();

        var allRequests = await _unitOfWork.MakeUpRequests.GetAllAsync();
        var pendingRequests = allRequests
            .Where(r => r.Status == MakeUpRequestStatus.Pending && allReviewableContractIds.Contains(r.ContractId))
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        var totalCount = pendingRequests.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
        var pagedItems = pendingRequests
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var items = new List<MakeUpRequestListDto>();
        foreach (var request in pagedItems)
        {
            var dto = await MapToMakeUpRequestListDto(request);
            items.Add(dto);
        }

        return new PagedResultDto<MakeUpRequestListDto>
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

    private async Task NotifySupervisorsForReview(Contract contract, MakeUpRequest request)
    {
        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        var allUsers = await _unitOfWork.Users.GetAllAsync();
        var applicant = allUsers.FirstOrDefault(u => u.Id == request.UserId);
        var applicantName = applicant?.Username ?? "未知用户";

        var supervisorIds = allPartners
            .Where(p => p.ContractId == contract.Id && p.Role == PartnerRole.Supervisor && p.Status == PartnerStatus.Accepted)
            .Select(p => p.PartnerId)
            .ToList();

        if (contract.OwnerId != request.UserId && !supervisorIds.Contains(contract.OwnerId))
        {
            supervisorIds.Add(contract.OwnerId);
        }

        var title = $"契约「{contract.HabitName}」补卡申请待审核";
        var content = $"用户「{applicantName}」提交了补卡申请，日期：{request.CheckInDate:yyyy-MM-dd}，原因：{request.Reason}。请及时审核。";

        foreach (var supervisorId in supervisorIds)
        {
            var supervisor = allUsers.FirstOrDefault(u => u.Id == supervisorId);
            if (supervisor == null) continue;

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

    private async Task NotifyApplicantReviewResult(MakeUpRequest request, Contract contract)
    {
        var allUsers = await _unitOfWork.Users.GetAllAsync();
        var applicant = allUsers.FirstOrDefault(u => u.Id == request.UserId);
        var reviewer = allUsers.FirstOrDefault(u => u.Id == request.ReviewedBy);
        var reviewerName = reviewer?.Username ?? "监督伙伴";

        if (applicant == null) return;

        string title, content;

        if (request.Status == MakeUpRequestStatus.Approved)
        {
            title = $"契约「{contract.HabitName}」补卡申请已通过";
            content = $"您的补卡申请（日期：{request.CheckInDate:yyyy-MM-dd}）已被「{reviewerName}」审核通过。打卡状态已更新为补卡。";
        }
        else
        {
            title = $"契约「{contract.HabitName}」补卡申请未通过";
            content = $"您的补卡申请（日期：{request.CheckInDate:yyyy-MM-dd}）未通过。审核人：{reviewerName}，拒绝原因：{request.RejectionReason}。";
        }

        foreach (var sender in _notificationSenders)
        {
            try
            {
                await sender.SendAsync(applicant, title, content);
            }
            catch
            {
            }
        }
    }

    private async Task<MakeUpRequestDto> MapToMakeUpRequestDto(MakeUpRequest request)
    {
        var dto = _mapper.Map<MakeUpRequestDto>(request);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(request.ContractId);
        dto.ContractName = contract?.HabitName;

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        dto.Username = user?.Username;

        if (request.ReviewedBy.HasValue)
        {
            var reviewer = await _unitOfWork.Users.GetByIdAsync(request.ReviewedBy.Value);
            dto.ReviewerName = reviewer?.Username;
        }

        return dto;
    }

    private async Task<MakeUpRequestListDto> MapToMakeUpRequestListDto(MakeUpRequest request)
    {
        var dto = _mapper.Map<MakeUpRequestListDto>(request);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(request.ContractId);
        dto.ContractName = contract?.HabitName;

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        dto.Username = user?.Username;

        return dto;
    }

    private async Task<PagedResultDto<MakeUpRequestListDto>> MapToPagedMakeUpRequestListDto(PagedResult<MakeUpRequest> pagedResult)
    {
        var items = new List<MakeUpRequestListDto>();
        foreach (var request in pagedResult.Items)
        {
            var dto = await MapToMakeUpRequestListDto(request);
            items.Add(dto);
        }

        return new PagedResultDto<MakeUpRequestListDto>
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
