using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;

namespace HabitContract.Api.Controllers;

[ApiController]
[Route("api/contractviolations")]
public class ContractViolationsController : ApiControllerBase
{
    private readonly IContractViolationService _contractViolationService;

    public ContractViolationsController(IContractViolationService contractViolationService)
    {
        _contractViolationService = contractViolationService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetViolations([FromQuery] QueryParameters parameters)
    {
        var result = await _contractViolationService.GetViolationsAsync(parameters);
        return Success(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateViolation([FromBody] ContractViolationCreateDto dto)
    {
        var result = await _contractViolationService.CreateViolationAsync(GetCurrentUserId(), dto);
        return Success(result, "创建成功");
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetViolationById(int id)
    {
        var result = await _contractViolationService.GetViolationByIdAsync(id);
        return Success(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateViolation(int id, [FromBody] ContractViolationUpdateDto dto)
    {
        var result = await _contractViolationService.UpdateViolationAsync(GetCurrentUserId(), id, dto);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteViolation(int id)
    {
        await _contractViolationService.DeleteViolationAsync(GetCurrentUserId(), id);
        return Success("删除成功");
    }
}
