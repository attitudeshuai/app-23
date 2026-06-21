using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;

namespace HabitContract.Api.Controllers;

[ApiController]
[Route("api/contracts")]
public class ContractsController : ApiControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetContracts([FromQuery] QueryParameters parameters)
    {
        var result = await _contractService.GetContractsAsync(parameters);
        return Success(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateContract([FromBody] ContractCreateDto dto)
    {
        var result = await _contractService.CreateContractAsync(GetCurrentUserId(), dto);
        return Success(result, "创建成功");
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetContractById(int id)
    {
        var result = await _contractService.GetContractByIdAsync(id);
        return Success(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateContract(int id, [FromBody] ContractUpdateDto dto)
    {
        var result = await _contractService.UpdateContractAsync(GetCurrentUserId(), id, dto);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteContract(int id)
    {
        await _contractService.DeleteContractAsync(GetCurrentUserId(), id);
        return Success("删除成功");
    }

    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateContractStatus(int id, [FromBody] ContractStatusDto dto)
    {
        var result = await _contractService.UpdateContractStatusAsync(GetCurrentUserId(), id, dto);
        return Success(result, "状态更新成功");
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMyContracts([FromQuery] QueryParameters parameters)
    {
        var result = await _contractService.GetMyContractsAsync(GetCurrentUserId(), parameters);
        return Success(result);
    }
}
