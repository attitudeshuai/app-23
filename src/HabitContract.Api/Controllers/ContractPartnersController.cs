using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;

namespace HabitContract.Api.Controllers;

[ApiController]
[Route("api/contractpartners")]
public class ContractPartnersController : ApiControllerBase
{
    private readonly IContractPartnerService _contractPartnerService;

    public ContractPartnersController(IContractPartnerService contractPartnerService)
    {
        _contractPartnerService = contractPartnerService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetPartners([FromQuery] QueryParameters parameters)
    {
        var result = await _contractPartnerService.GetPartnersAsync(parameters);
        return Success(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePartner([FromBody] ContractPartnerCreateDto dto)
    {
        var result = await _contractPartnerService.CreatePartnerAsync(GetCurrentUserId(), dto);
        return Success(result, "创建成功");
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetPartnerById(int id)
    {
        var result = await _contractPartnerService.GetPartnerByIdAsync(id);
        return Success(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePartner(int id, [FromBody] ContractPartnerUpdateDto dto)
    {
        var result = await _contractPartnerService.UpdatePartnerAsync(GetCurrentUserId(), id, dto);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePartner(int id)
    {
        await _contractPartnerService.DeletePartnerAsync(GetCurrentUserId(), id);
        return Success("删除成功");
    }

    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdatePartnerStatus(int id, [FromBody] ContractPartnerStatusDto dto)
    {
        var result = await _contractPartnerService.UpdatePartnerStatusAsync(GetCurrentUserId(), id, dto);
        return Success(result, "状态更新成功");
    }
}
