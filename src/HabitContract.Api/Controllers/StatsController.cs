using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HabitContract.Application.Interfaces;

namespace HabitContract.Api.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsController : ApiControllerBase
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet("overview")]
    [Authorize]
    public async Task<IActionResult> GetOverview()
    {
        var result = await _statsService.GetOverviewAsync();
        return Success(result);
    }

    [HttpGet("trend")]
    [Authorize]
    public async Task<IActionResult> GetTrend([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _statsService.GetTrendAsync(startDate, endDate);
        return Success(result);
    }

    [HttpGet("contract/{contractId}")]
    [Authorize]
    public async Task<IActionResult> GetContractStats(int contractId)
    {
        var result = await _statsService.GetContractStatsAsync(contractId, GetCurrentUserId());
        return Success(result);
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetUserStats()
    {
        var result = await _statsService.GetUserStatsAsync(GetCurrentUserId());
        return Success(result);
    }
}
