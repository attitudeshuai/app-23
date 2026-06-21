using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class ReminderService : IReminderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReminderService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ReminderService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ReminderService> logger,
        IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<ReminderSettingDto> CreateSettingAsync(int userId, ReminderSettingCreateDto dto)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(dto.ContractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在", 404);
        }

        if (contract.OwnerId != userId)
        {
            throw new BusinessException("无权限设置此契约的提醒", 403);
        }

        var existing = await _unitOfWork.ContractReminderSettings.GetByContractAndUserAsync(dto.ContractId, userId);
        if (existing != null)
        {
            throw new BusinessException("该契约已设置提醒，请修改现有设置");
        }

        if (dto.QuietStart.HasValue && dto.QuietEnd.HasValue)
        {
            if (dto.QuietStart.Value >= dto.QuietEnd.Value)
            {
                throw new BusinessException("静音开始时间必须早于结束时间");
            }
        }

        var setting = _mapper.Map<ContractReminderSetting>(dto);
        setting.UserId = userId;
        setting.CreatedAt = DateTime.UtcNow;

        var created = await _unitOfWork.ContractReminderSettings.AddAsync(setting);
        await _unitOfWork.SaveChangesAsync();

        return await MapToSettingDto(created);
    }

    public async Task<ReminderSettingDto> UpdateSettingAsync(int userId, int id, ReminderSettingUpdateDto dto)
    {
        var setting = await _unitOfWork.ContractReminderSettings.GetByContractAndUserAsync(id, userId);
        if (setting == null)
        {
            setting = (await _unitOfWork.ContractReminderSettings.GetByUserIdAsync(userId))
                .FirstOrDefault(s => s.Id == id);

            if (setting == null)
            {
                throw new BusinessException("提醒设置不存在或无权限修改", 404);
            }
        }

        if (dto.QuietStart.HasValue && dto.QuietEnd.HasValue)
        {
            if (dto.QuietStart.Value >= dto.QuietEnd.Value)
            {
                throw new BusinessException("静音开始时间必须早于结束时间");
            }
        }

        if (dto.ReminderTime.HasValue)
            setting.ReminderTime = dto.ReminderTime.Value;
        if (dto.IsEnabled.HasValue)
            setting.IsEnabled = dto.IsEnabled.Value;
        if (dto.QuietStart.HasValue)
            setting.QuietStart = dto.QuietStart.Value;
        if (dto.QuietEnd.HasValue)
            setting.QuietEnd = dto.QuietEnd.Value;
        if (dto.Channel.HasValue)
            setting.Channel = dto.Channel.Value;

        setting.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ContractReminderSettings.UpdateAsync(setting);
        await _unitOfWork.SaveChangesAsync();

        return await MapToSettingDto(setting);
    }

    public async Task DeleteSettingAsync(int userId, int id)
    {
        var settings = await _unitOfWork.ContractReminderSettings.GetByUserIdAsync(userId);
        var setting = settings.FirstOrDefault(s => s.Id == id);

        if (setting == null)
        {
            throw new BusinessException("提醒设置不存在或无权限删除", 404);
        }

        await _unitOfWork.ContractReminderSettings.DeleteAsync(setting);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ReminderSettingDto> GetSettingByIdAsync(int id)
    {
        var settings = await _unitOfWork.ContractReminderSettings.GetEnabledSettingsAsync();
        var setting = settings.FirstOrDefault(s => s.Id == id)
            ?? (await _unitOfWork.ContractReminderSettings.GetByContractIdAsync(0))
                .FirstOrDefault(s => s.Id == id);

        if (setting == null)
        {
            throw new BusinessException("提醒设置不存在", 404);
        }

        return await MapToSettingDto(setting);
    }

    public async Task<IEnumerable<ReminderSettingDto>> GetMySettingsAsync(int userId)
    {
        var settings = await _unitOfWork.ContractReminderSettings.GetByUserIdAsync(userId);
        var dtos = new List<ReminderSettingDto>();

        foreach (var setting in settings)
        {
            dtos.Add(await MapToSettingDto(setting));
        }

        return dtos;
    }

    public async Task<IEnumerable<ReminderSettingDto>> GetSettingsByContractAsync(int contractId)
    {
        var settings = await _unitOfWork.ContractReminderSettings.GetByContractIdAsync(contractId);
        var dtos = new List<ReminderSettingDto>();

        foreach (var setting in settings)
        {
            dtos.Add(await MapToSettingDto(setting));
        }

        return dtos;
    }

    public async Task DisableSettingAsync(int userId, int contractId)
    {
        var setting = await _unitOfWork.ContractReminderSettings.GetByContractAndUserAsync(contractId, userId);
        if (setting == null)
        {
            throw new BusinessException("该契约未设置提醒", 404);
        }

        setting.IsEnabled = false;
        setting.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ContractReminderSettings.UpdateAsync(setting);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ReminderRecordDto>> GetMyReminderRecordsAsync(int userId, QueryParameters parameters)
    {
        var records = await _unitOfWork.ReminderRecords.GetByUserIdAsync(userId);

        var pagedRecords = records
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize);

        var dtos = new List<ReminderRecordDto>();
        foreach (var record in pagedRecords)
        {
            dtos.Add(await MapToRecordDto(record));
        }

        return dtos;
    }

    public async Task<IEnumerable<ReminderRecordDto>> GetReminderRecordsByContractAsync(int contractId, QueryParameters parameters)
    {
        var records = await _unitOfWork.ReminderRecords.GetByContractIdAsync(contractId);

        var pagedRecords = records
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize);

        var dtos = new List<ReminderRecordDto>();
        foreach (var record in pagedRecords)
        {
            dtos.Add(await MapToRecordDto(record));
        }

        return dtos;
    }

    public async Task SubmitFeedbackAsync(int userId, ReminderFeedbackDto dto)
    {
        var record = await _unitOfWork.ReminderRecords.GetByIdAsync(dto.ReminderRecordId);
        if (record == null)
        {
            throw new BusinessException("提醒记录不存在", 404);
        }

        if (record.UserId != userId)
        {
            throw new BusinessException("无权限对此提醒提交反馈", 403);
        }

        if (dto.DisableFutureReminders)
        {
            var setting = await _unitOfWork.ContractReminderSettings.GetByContractAndUserAsync(
                record.ContractId, userId);
            if (setting != null)
            {
                setting.IsEnabled = false;
                setting.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.ContractReminderSettings.UpdateAsync(setting);
            }
        }

        _logger.LogInformation(
            "用户 {UserId} 对提醒 {ReminderRecordId} 提交反馈: {Feedback}",
            userId, dto.ReminderRecordId, dto.Feedback);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ProcessRemindersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("开始处理提醒任务...");

        try
        {
            var now = DateTime.Now;
            var today = now.Date;

            var settings = await _unitOfWork.ContractReminderSettings.GetEnabledSettingsAsync();

            foreach (var setting in settings)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    if (!ShouldSendReminder(setting, now))
                    {
                        continue;
                    }

                    var hasCheckedIn = await HasCheckedInToday(setting.ContractId, setting.UserId, today);
                    if (hasCheckedIn)
                    {
                        continue;
                    }

                    var hasSent = await _unitOfWork.ReminderRecords.HasSentTodayAsync(
                        setting.ContractId, setting.UserId, today);
                    if (hasSent)
                    {
                        continue;
                    }

                    await SendReminderAsync(setting, today);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理契约 {ContractId} 的提醒时出错", setting.ContractId);
                }
            }

            _logger.LogInformation("提醒任务处理完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理提醒任务时发生严重错误");
            throw;
        }
    }

    private bool ShouldSendReminder(ContractReminderSetting setting, DateTime now)
    {
        var currentTime = now.TimeOfDay;

        if (setting.QuietStart.HasValue && setting.QuietEnd.HasValue)
        {
            if (currentTime >= setting.QuietStart.Value && currentTime < setting.QuietEnd.Value)
            {
                return false;
            }
        }

        var reminderTime = setting.ReminderTime;
        var tolerance = TimeSpan.FromMinutes(5);
        var timeDiff = currentTime - reminderTime;

        return timeDiff >= TimeSpan.Zero && timeDiff <= tolerance;
    }

    private async Task<bool> HasCheckedInToday(int contractId, int userId, DateTime date)
    {
        var checkIns = await _unitOfWork.CheckIns.GetAllAsync();
        var dateStart = date.Date;
        var dateEnd = dateStart.AddDays(1);

        return checkIns.Any(ci =>
            ci.ContractId == contractId &&
            ci.UserId == userId &&
            ci.CheckInDate >= dateStart &&
            ci.CheckInDate < dateEnd);
    }

    private async Task SendReminderAsync(ContractReminderSetting setting, DateTime reminderDate)
    {
        var template = await _unitOfWork.ReminderTemplates.GetDefaultAsync(ReminderTemplateType.DefaultCheckInReminder)
            ?? await _unitOfWork.ReminderTemplates.GetByTypeAsync(ReminderTemplateType.DefaultCheckInReminder);

        var contract = setting.Contract;
        var user = setting.User;

        var templateData = new
        {
            UserName = user.Username,
            HabitName = contract.HabitName,
            ContractId = contract.Id,
            Date = reminderDate.ToString("yyyy-MM-dd"),
            Penalty = contract.PenaltyDescription ?? "未设置"
        };

        var title = template != null
            ? RenderTemplateContent(template.TitleTemplate, templateData)
            : $"提醒：今日「{contract.HabitName}」还未打卡";

        var content = template != null
            ? RenderTemplateContent(template.ContentTemplate, templateData)
            : $"你好 {user.Username}，今天是 {reminderDate:yyyy-MM-dd}，「{contract.HabitName}」还未打卡，请及时完成打卡任务。";

        var contractInfo = $"契约: {contract.HabitName}, 频率: {contract.Frequency}";

        var record = new ReminderRecord
        {
            ContractId = setting.ContractId,
            UserId = setting.UserId,
            SettingId = setting.Id,
            ReminderDate = reminderDate,
            Title = title,
            Content = content,
            Channel = setting.Channel,
            Status = ReminderStatus.Pending,
            ContractInfo = contractInfo,
            CreatedAt = DateTime.UtcNow
        };

        var createdRecord = await _unitOfWork.ReminderRecords.AddAsync(record);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            var sender = GetNotificationSender(setting.Channel);
            var success = await sender.SendAsync(user, title, content);

            createdRecord.Status = success ? ReminderStatus.Sent : ReminderStatus.Failed;
            createdRecord.SentAt = DateTime.UtcNow;
            if (!success)
            {
                createdRecord.ErrorMessage = "通知发送失败";
            }
        }
        catch (Exception ex)
        {
            createdRecord.Status = ReminderStatus.Failed;
            createdRecord.ErrorMessage = ex.Message;
            _logger.LogError(ex, "发送提醒失败: {RecordId}", createdRecord.Id);
        }

        await _unitOfWork.ReminderRecords.UpdateAsync(createdRecord);
        await _unitOfWork.SaveChangesAsync();
    }

    private INotificationSender GetNotificationSender(ReminderChannel channel)
    {
        var senders = _serviceProvider.GetServices<INotificationSender>();
        var sender = senders.FirstOrDefault(s => s.Channel == channel);

        if (sender == null)
        {
            _logger.LogWarning("未找到通道 {Channel} 的通知发送器，使用默认站内信发送器", channel);
            sender = senders.First(s => s.Channel == ReminderChannel.InApp);
        }

        return sender;
    }

    private string RenderTemplateContent(string template, object data)
    {
        var result = template;
        var properties = data.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var pattern = $"{{{prop.Name}}}";
            var value = prop.GetValue(data)?.ToString() ?? string.Empty;
            result = Regex.Replace(result, pattern, value, RegexOptions.IgnoreCase);
        }

        return result;
    }

    private async Task<ReminderSettingDto> MapToSettingDto(ContractReminderSetting setting)
    {
        var dto = _mapper.Map<ReminderSettingDto>(setting);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(setting.ContractId);
        dto.ContractName = contract?.HabitName;

        var user = await _unitOfWork.Users.GetByIdAsync(setting.UserId);
        dto.Username = user?.Username;

        return dto;
    }

    private async Task<ReminderRecordDto> MapToRecordDto(ReminderRecord record)
    {
        var dto = _mapper.Map<ReminderRecordDto>(record);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(record.ContractId);
        dto.ContractName = contract?.HabitName;

        var user = await _unitOfWork.Users.GetByIdAsync(record.UserId);
        dto.Username = user?.Username;

        return dto;
    }
}
