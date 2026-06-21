using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;

namespace HabitContract.Api.Controllers;

[ApiController]
[Route("api/checkins")]
public class CheckInsController : ApiControllerBase
{
    private readonly ICheckInService _checkInService;

    public CheckInsController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCheckIns([FromQuery] QueryParameters parameters)
    {
        var result = await _checkInService.GetCheckInsAsync(parameters);
        return Success(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateCheckIn([FromBody] CheckInCreateDto dto)
    {
        var result = await _checkInService.CreateCheckInAsync(GetCurrentUserId(), dto);
        return Success(result, "打卡成功");
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetCheckInById(int id)
    {
        var result = await _checkInService.GetCheckInByIdAsync(id);
        return Success(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateCheckIn(int id, [FromBody] CheckInUpdateDto dto)
    {
        var result = await _checkInService.UpdateCheckInAsync(GetCurrentUserId(), id, dto);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteCheckIn(int id)
    {
        await _checkInService.DeleteCheckInAsync(GetCurrentUserId(), id);
        return Success("删除成功");
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMyCheckIns([FromQuery] QueryParameters parameters)
    {
        var result = await _checkInService.GetMyCheckInsAsync(GetCurrentUserId(), parameters);
        return Success(result);
    }
}
