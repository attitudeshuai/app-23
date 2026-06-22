using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;

namespace HabitContract.Api.Controllers;

[ApiController]
[Route("api/penalties")]
public class PenaltiesController : ApiControllerBase
{
    private readonly IPenaltyService _penaltyService;

    public PenaltiesController(IPenaltyService penaltyService)
    {
        _penaltyService = penaltyService;
    }

    [HttpGet("rules")]
    [Authorize]
    public async Task<IActionResult> GetPenaltyRules([FromQuery] QueryParameters parameters)
    {
        var result = await _penaltyService.GetPenaltyRulesAsync(parameters);
        return Success(result);
    }

    [HttpGet("rules/{id}")]
    [Authorize]
    public async Task<IActionResult> GetPenaltyRuleById(int id)
    {
        var result = await _penaltyService.GetPenaltyRuleByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "惩罚规则不存在" });
        }
        return Success(result);
    }

    [HttpGet("rules/contract/{contractId}")]
    [Authorize]
    public async Task<IActionResult> GetPenaltyRulesByContractId(int contractId)
    {
        var result = await _penaltyService.GetPenaltyRulesByContractIdAsync(contractId);
        return Success(result);
    }

    [HttpGet("rules/default/{contractId}")]
    [Authorize]
    public async Task<IActionResult> GetDefaultPenaltyConfig(int contractId)
    {
        var result = await _penaltyService.GetDefaultPenaltyConfigAsync(contractId);
        return Success(result);
    }

    [HttpPost("rules")]
    [Authorize]
    public async Task<IActionResult> CreatePenaltyRule([FromBody] PenaltyRuleCreateDto dto)
    {
        var result = await _penaltyService.CreatePenaltyRuleAsync(GetCurrentUserId(), dto);
        return Success(result, "惩罚规则创建成功");
    }

    [HttpPut("rules/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePenaltyRule(int id, [FromBody] PenaltyRuleUpdateDto dto)
    {
        var result = await _penaltyService.UpdatePenaltyRuleAsync(GetCurrentUserId(), id, dto);
        return Success(result, "惩罚规则更新成功");
    }

    [HttpDelete("rules/{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePenaltyRule(int id)
    {
        await _penaltyService.DeletePenaltyRuleAsync(GetCurrentUserId(), id);
        return Success("惩罚规则删除成功");
    }

    [HttpPost("rules/supplement/{contractId}")]
    [Authorize]
    public async Task<IActionResult> SupplementPenaltyRule(int contractId, [FromBody] PenaltyRuleCreateDto dto)
    {
        var result = await _penaltyService.SupplementPenaltyRuleAsync(GetCurrentUserId(), contractId, dto);
        return Success(result, "惩罚规则补充配置成功");
    }

    [HttpGet("executions")]
    [Authorize]
    public async Task<IActionResult> GetExecutionRecords([FromQuery] QueryParameters parameters)
    {
        var result = await _penaltyService.GetExecutionRecordsAsync(parameters);
        return Success(result);
    }

    [HttpGet("executions/{id}")]
    [Authorize]
    public async Task<IActionResult> GetExecutionRecordById(int id)
    {
        var result = await _penaltyService.GetExecutionRecordByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "惩罚执行记录不存在" });
        }
        return Success(result);
    }

    [HttpGet("executions/contract/{contractId}")]
    [Authorize]
    public async Task<IActionResult> GetExecutionRecordsByContractId(int contractId)
    {
        var result = await _penaltyService.GetExecutionRecordsByContractIdAsync(contractId);
        return Success(result);
    }

    [HttpGet("executions/user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetExecutionRecordsByUserId(int userId)
    {
        var result = await _penaltyService.GetExecutionRecordsByUserIdAsync(userId);
        return Success(result);
    }

    [HttpGet("executions/contract/{contractId}/user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetExecutionRecordsByContractIdAndUserId(int contractId, int userId)
    {
        var result = await _penaltyService.GetExecutionRecordsByContractIdAndUserIdAsync(contractId, userId);
        return Success(result);
    }

    [HttpGet("executions/mine")]
    [Authorize]
    public async Task<IActionResult> GetMyExecutionRecords()
    {
        var result = await _penaltyService.GetExecutionRecordsByUserIdAsync(GetCurrentUserId());
        return Success(result);
    }

    [HttpPost("executions")]
    [Authorize]
    public async Task<IActionResult> CreateExecutionRecord([FromBody] PenaltyExecutionCreateDto dto)
    {
        var result = await _penaltyService.CreateExecutionRecordAsync(GetCurrentUserId(), dto);
        return Success(result, "惩罚执行记录创建成功");
    }

    [HttpPut("executions/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateExecutionRecord(int id, [FromBody] PenaltyExecutionUpdateDto dto)
    {
        var result = await _penaltyService.UpdateExecutionRecordAsync(GetCurrentUserId(), id, dto);
        return Success(result, "惩罚执行记录更新成功");
    }

    [HttpPost("executions/{id}/waive")]
    [Authorize]
    public async Task<IActionResult> WaiveExecutionRecord(int id, [FromBody] PenaltyExecutionWaiveDto dto)
    {
        var result = await _penaltyService.WaiveExecutionRecordAsync(GetCurrentUserId(), id, dto);
        return Success(result, "惩罚已豁免");
    }

    [HttpGet("overview")]
    [Authorize]
    public async Task<IActionResult> GetOverview()
    {
        var result = await _penaltyService.GetOverviewAsync();
        return Success(result);
    }

    [HttpGet("trend")]
    [Authorize]
    public async Task<IActionResult> GetTrend([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _penaltyService.GetTrendAsync(startDate, endDate);
        return Success(result);
    }
}
