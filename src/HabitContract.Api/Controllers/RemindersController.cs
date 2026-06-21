using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;

namespace HabitContract.Api.Controllers;

[ApiController]
[Route("api/reminders")]
public class RemindersController : ApiControllerBase
{
    private readonly IReminderService _reminderService;

    public RemindersController(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    [HttpGet("settings")]
    [Authorize]
    public async Task<IActionResult> GetMySettings()
    {
        var result = await _reminderService.GetMySettingsAsync(GetCurrentUserId());
        return Success(result);
    }

    [HttpGet("settings/contract/{contractId}")]
    [Authorize]
    public async Task<IActionResult> GetSettingsByContract(int contractId)
    {
        var result = await _reminderService.GetSettingsByContractAsync(contractId);
        return Success(result);
    }

    [HttpGet("settings/{id}")]
    [Authorize]
    public async Task<IActionResult> GetSettingById(int id)
    {
        var result = await _reminderService.GetSettingByIdAsync(id);
        return Success(result);
    }

    [HttpPost("settings")]
    [Authorize]
    public async Task<IActionResult> CreateSetting([FromBody] ReminderSettingCreateDto dto)
    {
        var result = await _reminderService.CreateSettingAsync(GetCurrentUserId(), dto);
        return Success(result, "提醒设置创建成功");
    }

    [HttpPut("settings/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateSetting(int id, [FromBody] ReminderSettingUpdateDto dto)
    {
        var result = await _reminderService.UpdateSettingAsync(GetCurrentUserId(), id, dto);
        return Success(result, "提醒设置更新成功");
    }

    [HttpDelete("settings/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteSetting(int id)
    {
        await _reminderService.DeleteSettingAsync(GetCurrentUserId(), id);
        return Success("提醒设置删除成功");
    }

    [HttpPost("settings/disable/{contractId}")]
    [Authorize]
    public async Task<IActionResult> DisableSetting(int contractId)
    {
        await _reminderService.DisableSettingAsync(GetCurrentUserId(), contractId);
        return Success("提醒已关闭");
    }

    [HttpGet("records")]
    [Authorize]
    public async Task<IActionResult> GetMyRecords([FromQuery] QueryParameters parameters)
    {
        var result = await _reminderService.GetMyReminderRecordsAsync(GetCurrentUserId(), parameters);
        return Success(result);
    }

    [HttpGet("records/contract/{contractId}")]
    [Authorize]
    public async Task<IActionResult> GetRecordsByContract(int contractId, [FromQuery] QueryParameters parameters)
    {
        var result = await _reminderService.GetReminderRecordsByContractAsync(contractId, parameters);
        return Success(result);
    }

    [HttpPost("feedback")]
    [Authorize]
    public async Task<IActionResult> SubmitFeedback([FromBody] ReminderFeedbackDto dto)
    {
        await _reminderService.SubmitFeedbackAsync(GetCurrentUserId(), dto);
        return Success("反馈提交成功");
    }
}
