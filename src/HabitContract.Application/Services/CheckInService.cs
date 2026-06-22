using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class CheckInService : ICheckInService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFrequencyRuleCache _frequencyCache;
    private readonly IEnumerable<INotificationSender> _notificationSenders;
    private readonly IPermissionService _permissionService;

    public CheckInService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFrequencyRuleCache frequencyCache,
        IEnumerable<INotificationSender> notificationSenders,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _frequencyCache = frequencyCache;
        _notificationSenders = notificationSenders;
        _permissionService = permissionService;
    }

    public async Task<PagedResultDto<CheckInListDto>> GetCheckInsAsync(QueryParameters parameters)
    {
        var pagedResult = await _unitOfWork.CheckIns.GetPagedAsync(parameters);
        return await MapToPagedCheckInListDto(pagedResult);
    }

    public async Task<CheckInDto> GetCheckInByIdAsync(int userId, int id)
    {
        var checkIn = await _unitOfWork.CheckIns.GetByIdAsync(id);
        if (checkIn == null)
        {
            throw new BusinessException("打卡记录不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, checkIn.ContractId, ContractOperation.ViewCheckIns);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        return await MapToCheckInDto(checkIn);
    }

    public async Task<CheckInDto> CreateCheckInAsync(int userId, CheckInCreateDto dto)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(dto.ContractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在");
        }

        if (contract.Status != ContractStatus.Active)
        {
            throw new BusinessException("只能为进行中的契约打卡");
        }

        var frequencyRule = await _frequencyCache.GetOrCreateAsync(contract.Id, contract.Frequency);
        var checkInDate = dto.CheckInDate.Date;
        var checkInTime = dto.CheckInTime == default ? DateTime.UtcNow : dto.CheckInTime;

        var currentPeriodCount = GetCurrentPeriodCheckInCount(contract.Id, userId, frequencyRule, checkInDate);
        var validationResult = ValidateCheckInAgainstRule(frequencyRule, checkInDate, currentPeriodCount);

        if (!validationResult.IsValid)
        {
            await RecordViolationAndNotifyPartners(contract, userId, checkInDate, validationResult.ErrorMessage);
            throw new BusinessException(validationResult.ErrorMessage);
        }

        var checkIn = _mapper.Map<CheckIn>(dto);
        checkIn.UserId = userId;
        checkIn.CheckInDate = checkInDate;
        checkIn.CheckInTime = checkInTime;

        var status = DetermineCheckInStatus(contract, checkInDate, checkInTime);
        checkIn.Status = status;
        checkIn.StatusChangedAt = DateTime.UtcNow;

        if (status != CheckInStatus.Missed)
        {
            checkIn.ConsecutiveDays = await _unitOfWork.CheckIns.GetConsecutiveDaysAsync(contract.Id, userId, checkInDate);
        }

        var created = await _unitOfWork.CheckIns.AddAsync(checkIn);
        await _unitOfWork.SaveChangesAsync();

        await UpdateSubsequentCheckInConsecutiveDays(contract.Id, userId, checkInDate);

        return await MapToCheckInDto(created);
    }

    private CheckInStatus DetermineCheckInStatus(Contract contract, DateTime checkInDate, DateTime checkInTime)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(contract.TimeZone);
        var checkInLocalTime = TimeZoneInfo.ConvertTimeFromUtc(checkInTime, timeZone);
        var deadlineLocalTime = checkInDate.Date.Add(contract.CheckInDeadline);

        if (checkInLocalTime <= deadlineLocalTime)
        {
            return CheckInStatus.Normal;
        }

        var makeUpDeadline = checkInDate.Date.AddDays(contract.MakeUpDeadlineDays);

        if (checkInLocalTime.Date <= makeUpDeadline)
        {
            return CheckInStatus.MakeUp;
        }

        return CheckInStatus.Missed;
    }

    private async Task UpdateSubsequentCheckInConsecutiveDays(int contractId, int userId, DateTime fromDate)
    {
        var allCheckIns = await _unitOfWork.CheckIns.GetByContractAndUserIdAsync(contractId, userId);
        var subsequentCheckIns = allCheckIns?
            .Where(ci => ci.CheckInDate > fromDate.Date && ci.Status != CheckInStatus.Missed)
            .OrderBy(ci => ci.CheckInDate)
            .ToList() ?? new List<CheckIn>();

        foreach (var checkIn in subsequentCheckIns)
        {
            checkIn.ConsecutiveDays = await _unitOfWork.CheckIns.GetConsecutiveDaysAsync(contractId, userId, checkIn.CheckInDate);
            await _unitOfWork.CheckIns.UpdateAsync(checkIn);
        }

        if (subsequentCheckIns.Any())
        {
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<CheckInDto> UpdateCheckInAsync(int userId, int id, CheckInUpdateDto dto)
    {
        var checkIn = await _unitOfWork.CheckIns.GetByIdAsync(id);
        if (checkIn == null)
        {
            throw new BusinessException("打卡记录不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, checkIn.ContractId, ContractOperation.EditCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        if (checkIn.UserId != userId)
        {
            throw new BusinessException("无权限修改此打卡记录", 403);
        }

        if (dto.ProofText != null)
            checkIn.ProofText = dto.ProofText;

        if (dto.ProofPhoto != null)
            checkIn.ProofPhoto = dto.ProofPhoto;

        checkIn.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.CheckIns.UpdateAsync(checkIn);
        await _unitOfWork.SaveChangesAsync();

        return await MapToCheckInDto(checkIn);
    }

    public async Task DeleteCheckInAsync(int userId, int id)
    {
        var checkIn = await _unitOfWork.CheckIns.GetByIdAsync(id);
        if (checkIn == null)
        {
            throw new BusinessException("打卡记录不存在", 404);
        }

        var (isAllowed, errorMsg) = await _permissionService.CheckPermissionAsync(userId, checkIn.ContractId, ContractOperation.DeleteCheckIn);
        if (!isAllowed)
        {
            throw new BusinessException(errorMsg, 403);
        }

        if (checkIn.UserId != userId)
        {
            throw new BusinessException("无权限删除此打卡记录", 403);
        }

        await _unitOfWork.CheckIns.DeleteAsync(checkIn);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResultDto<CheckInListDto>> GetMyCheckInsAsync(int userId, QueryParameters parameters)
    {
        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        var myCheckIns = allCheckIns.Where(ci => ci.UserId == userId).ToList();

        var totalCount = myCheckIns.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
        var pagedItems = myCheckIns
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var items = new List<CheckInListDto>();
        foreach (var checkIn in pagedItems)
        {
            var dto = await MapToCheckInListDto(checkIn);
            items.Add(dto);
        }

        return new PagedResultDto<CheckInListDto>
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

    private int GetCurrentPeriodCheckInCount(int contractId, int userId, FrequencyRule rule, DateTime checkInDate)
    {
        var allCheckIns = _unitOfWork.CheckIns.GetAllAsync().Result;
        var periodStart = GetPeriodStart(rule.Type, checkInDate);
        var periodEnd = GetPeriodEnd(rule.Type, checkInDate);

        return allCheckIns.Count(ci =>
            ci.ContractId == contractId &&
            ci.UserId == userId &&
            ci.CheckInDate >= periodStart &&
            ci.CheckInDate < periodEnd);
    }

    private static DateTime GetPeriodStart(FrequencyType type, DateTime date)
    {
        if (type == FrequencyType.Daily)
        {
            return date.Date;
        }

        var dayOfWeek = date.DayOfWeek;
        var diff = dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;
        return date.AddDays(-diff).Date;
    }

    private static DateTime GetPeriodEnd(FrequencyType type, DateTime date)
    {
        if (type == FrequencyType.Daily)
        {
            return date.Date.AddDays(1);
        }

        var periodStart = GetPeriodStart(FrequencyType.Weekly, date);
        return periodStart.AddDays(7);
    }

    private static FrequencyValidationResult ValidateCheckInAgainstRule(FrequencyRule rule, DateTime checkInDate, int currentCount)
    {
        if (rule.Type == FrequencyType.Daily)
        {
            return ValidateDailyCheckIn(rule, currentCount);
        }

        return ValidateWeeklyCheckIn(rule, checkInDate, currentCount);
    }

    private static FrequencyValidationResult ValidateDailyCheckIn(FrequencyRule rule, int currentCount)
    {
        var result = new FrequencyValidationResult
        {
            CurrentCount = currentCount,
            RequiredCount = rule.Count,
            Period = "今日"
        };

        if (currentCount >= rule.Count)
        {
            result.IsValid = false;
            result.ErrorMessage = $"今日打卡次数已达上限（{currentCount}/{rule.Count}次）";
            return result;
        }

        result.IsValid = true;
        return result;
    }

    private static FrequencyValidationResult ValidateWeeklyCheckIn(FrequencyRule rule, DateTime checkInDate, int currentCount)
    {
        var result = new FrequencyValidationResult
        {
            CurrentCount = currentCount,
            RequiredCount = rule.Count,
            Period = "本周"
        };

        if (rule.DaysOfWeek != null && rule.DaysOfWeek.Count > 0)
        {
            if (!rule.DaysOfWeek.Contains(checkInDate.DayOfWeek))
            {
                result.IsValid = false;
                var allowedDays = string.Join("、", rule.DaysOfWeek.Select(d => GetChineseDayName(d)));
                result.ErrorMessage = $"今天不允许打卡，仅允许在{allowedDays}打卡";
                return result;
            }
        }

        if (currentCount >= rule.Count)
        {
            result.IsValid = false;
            result.ErrorMessage = $"本周打卡次数已达上限（{currentCount}/{rule.Count}次）";
            return result;
        }

        result.IsValid = true;
        return result;
    }

    private static string GetChineseDayName(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => "周一",
            DayOfWeek.Tuesday => "周二",
            DayOfWeek.Wednesday => "周三",
            DayOfWeek.Thursday => "周四",
            DayOfWeek.Friday => "周五",
            DayOfWeek.Saturday => "周六",
            DayOfWeek.Sunday => "周日",
            _ => day.ToString()
        };
    }

    private async Task RecordViolationAndNotifyPartners(Contract contract, int userId, DateTime violationDate, string reason)
    {
        var violation = new ContractViolation
        {
            ContractId = contract.Id,
            ViolationDate = violationDate,
            Reason = reason,
            IsConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ContractViolations.AddAsync(violation);
        await _unitOfWork.SaveChangesAsync();
        await NotifyPartnersAsync(contract, violation, userId);
    }

    private async Task NotifyPartnersAsync(Contract contract, ContractViolation violation, int violatorId)
    {
        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        var partnerIds = allPartners
            .Where(p => p.ContractId == contract.Id && p.Status == PartnerStatus.Accepted && p.PartnerId != violatorId)
            .Select(p => p.PartnerId)
            .ToList();

        if (contract.OwnerId != violatorId && !partnerIds.Contains(contract.OwnerId))
        {
            partnerIds.Add(contract.OwnerId);
        }

        if (!partnerIds.Any())
        {
            return;
        }

        var allUsers = await _unitOfWork.Users.GetAllAsync();
        var violator = allUsers.FirstOrDefault(u => u.Id == violatorId);
        var violatorName = violator?.Username ?? "未知用户";

        foreach (var partnerId in partnerIds)
        {
            var partner = allUsers.FirstOrDefault(u => u.Id == partnerId);
            if (partner == null)
            {
                continue;
            }

            var title = $"契约「{contract.HabitName}」违约提醒";
            var content = $"用户「{violatorName}」在打卡时违反了频率规则：{violation.Reason}。请关注监督。";

            foreach (var sender in _notificationSenders)
            {
                try
                {
                    await sender.SendAsync(partner, title, content);
                }
                catch
                {
                }
            }
        }
    }

    public async Task ReValidateRecentCheckInsAsync(int contractId, FrequencyRule newRule)
    {
        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        var recentDate = DateTime.UtcNow.AddDays(-7);
        var recentCheckIns = allCheckIns
            .Where(ci => ci.ContractId == contractId && ci.CheckInDate >= recentDate)
            .OrderByDescending(ci => ci.CheckInDate)
            .ToList();

        var userGroups = recentCheckIns.GroupBy(ci => ci.UserId);

        foreach (var group in userGroups)
        {
            var userId = group.Key;
            var checkInsByDate = group.OrderBy(ci => ci.CheckInDate).ToList();
            var periods = new Dictionary<DateTime, List<CheckIn>>();

            foreach (var checkIn in checkInsByDate)
            {
                var periodStart = GetPeriodStart(newRule.Type, checkIn.CheckInDate);
                if (!periods.ContainsKey(periodStart))
                {
                    periods[periodStart] = new List<CheckIn>();
                }
                periods[periodStart].Add(checkIn);
            }

            foreach (var period in periods)
            {
                var periodCheckIns = period.Value;
                for (int i = 0; i < periodCheckIns.Count; i++)
                {
                    var validationResult = ValidateCheckInAgainstRule(newRule, periodCheckIns[i].CheckInDate, i);
                    if (!validationResult.IsValid)
                    {
                        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
                        if (contract != null)
                        {
                            await RecordViolationAndNotifyPartners(contract, userId, periodCheckIns[i].CheckInDate,
                                $"规则变更后校验失败：{validationResult.ErrorMessage}");
                        }
                        break;
                    }
                }
            }
        }
    }

    public async Task<int> UpdateMissedCheckInStatusesAsync()
    {
        var updatedCount = 0;
        var pendingRequests = await _unitOfWork.MakeUpRequests.GetPendingRequestsAsync();
        var now = DateTime.UtcNow;

        foreach (var request in pendingRequests)
        {
            var contract = request.Contract;
            if (contract == null) continue;

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(contract.TimeZone);
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(now, timeZone);
            var makeUpDeadline = request.CheckInDate.Date.AddDays(contract.MakeUpDeadlineDays);

            if (nowLocal.Date > makeUpDeadline)
            {
                request.Status = MakeUpRequestStatus.Rejected;
                request.ReviewedAt = now;
                request.RejectionReason = "补卡申请已超过审核期限，系统自动拒绝";
                await _unitOfWork.MakeUpRequests.UpdateAsync(request);

                var existingCheckIn = await _unitOfWork.CheckIns.GetByDateAsync(
                    request.ContractId, request.UserId, request.CheckInDate);
                if (existingCheckIn != null && existingCheckIn.Status != CheckInStatus.Missed)
                {
                    existingCheckIn.Status = CheckInStatus.Missed;
                    existingCheckIn.StatusChangedAt = now;
                    existingCheckIn.ConsecutiveDays = 0;
                    await _unitOfWork.CheckIns.UpdateAsync(existingCheckIn);

                    var allCheckIns = await _unitOfWork.CheckIns.GetByContractAndUserIdAsync(
                        request.ContractId, request.UserId);
                    var subsequentCheckIns = allCheckIns?
                        .Where(ci => ci.CheckInDate > request.CheckInDate && ci.Status != CheckInStatus.Missed)
                        .OrderBy(ci => ci.CheckInDate)
                        .ToList() ?? new List<CheckIn>();

                    foreach (var checkIn in subsequentCheckIns)
                    {
                        checkIn.ConsecutiveDays = await _unitOfWork.CheckIns.GetConsecutiveDaysAsync(
                            request.ContractId, request.UserId, checkIn.CheckInDate);
                        await _unitOfWork.CheckIns.UpdateAsync(checkIn);
                    }
                }

                updatedCount++;
            }
        }

        await _unitOfWork.CheckIns.UpdateStatusForMissedDeadlinesAsync();
        await _unitOfWork.SaveChangesAsync();

        return updatedCount;
    }

    public async Task<(int CurrentStreak, int LongestStreak)> GetStreaksAsync(int contractId, int userId)
    {
        return await _unitOfWork.CheckIns.GetStreaksAsync(contractId, userId);
    }

    private async Task<CheckInDto> MapToCheckInDto(CheckIn checkIn)
    {
        var dto = _mapper.Map<CheckInDto>(checkIn);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(checkIn.ContractId);
        dto.ContractName = contract?.HabitName;

        var user = await _unitOfWork.Users.GetByIdAsync(checkIn.UserId);
        dto.Username = user?.Username;

        return dto;
    }

    private async Task<CheckInListDto> MapToCheckInListDto(CheckIn checkIn)
    {
        var dto = _mapper.Map<CheckInListDto>(checkIn);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(checkIn.ContractId);
        dto.ContractName = contract?.HabitName;

        var user = await _unitOfWork.Users.GetByIdAsync(checkIn.UserId);
        dto.Username = user?.Username;

        return dto;
    }

    private async Task<PagedResultDto<CheckInListDto>> MapToPagedCheckInListDto(PagedResult<CheckIn> pagedResult)
    {
        var items = new List<CheckInListDto>();
        foreach (var checkIn in pagedResult.Items)
        {
            var dto = await MapToCheckInListDto(checkIn);
            items.Add(dto);
        }

        return new PagedResultDto<CheckInListDto>
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
