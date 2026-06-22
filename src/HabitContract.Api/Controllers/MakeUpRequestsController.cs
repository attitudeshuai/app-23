using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;

namespace HabitContract.Api.Controllers;

[ApiController]
[Route("api/makeup-requests")]
public class MakeUpRequestsController : ApiControllerBase
{
    private readonly IMakeUpRequestService _makeUpRequestService;

    public MakeUpRequestsController(IMakeUpRequestService makeUpRequestService)
    {
        _makeUpRequestService = makeUpRequestService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMakeUpRequests([FromQuery] QueryParameters parameters)
    {
        var result = await _makeUpRequestService.GetMakeUpRequestsAsync(parameters);
        return Success(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetMakeUpRequestById(int id)
    {
        var result = await _makeUpRequestService.GetMakeUpRequestByIdAsync(GetCurrentUserId(), id);
        return Success(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMakeUpRequest([FromBody] MakeUpRequestCreateDto dto)
    {
        var result = await _makeUpRequestService.CreateMakeUpRequestAsync(GetCurrentUserId(), dto);
        return Success(result, "补卡申请提交成功，请等待监督伙伴审核");
    }

    [HttpPut("{id}/review")]
    [Authorize]
    public async Task<IActionResult> ReviewMakeUpRequest(int id, [FromBody] MakeUpRequestReviewDto dto)
    {
        var result = await _makeUpRequestService.ReviewMakeUpRequestAsync(GetCurrentUserId(), id, dto);
        var message = dto.Status == Domain.Enums.MakeUpRequestStatus.Approved
            ? "补卡申请已通过，打卡状态已更新"
            : "补卡申请已拒绝，已通知申请人";
        return Success(result, message);
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMyMakeUpRequests([FromQuery] QueryParameters parameters)
    {
        var result = await _makeUpRequestService.GetMyMakeUpRequestsAsync(GetCurrentUserId(), parameters);
        return Success(result);
    }

    [HttpGet("pending-reviews")]
    [Authorize]
    public async Task<IActionResult> GetPendingReviews([FromQuery] QueryParameters parameters)
    {
        var result = await _makeUpRequestService.GetPendingReviewsAsync(GetCurrentUserId(), parameters);
        return Success(result);
    }
}
