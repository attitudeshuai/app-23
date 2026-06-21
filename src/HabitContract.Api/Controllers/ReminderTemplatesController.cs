using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;

namespace HabitContract.Api.Controllers;

[ApiController]
[Route("api/reminder-templates")]
public class ReminderTemplatesController : ApiControllerBase
{
    private readonly IReminderTemplateService _templateService;

    public ReminderTemplatesController(IReminderTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllTemplates()
    {
        var result = await _templateService.GetAllTemplatesAsync();
        return Success(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetTemplateById(int id)
    {
        var result = await _templateService.GetTemplateByIdAsync(id);
        return Success(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateTemplate([FromBody] ReminderTemplateCreateDto dto)
    {
        var result = await _templateService.CreateTemplateAsync(dto);
        return Success(result, "模板创建成功");
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTemplate(int id, [FromBody] ReminderTemplateUpdateDto dto)
    {
        var result = await _templateService.UpdateTemplateAsync(id, dto);
        return Success(result, "模板更新成功");
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTemplate(int id)
    {
        await _templateService.DeleteTemplateAsync(id);
        return Success("模板删除成功");
    }

    [HttpPost("{id}/render")]
    [Authorize]
    public async Task<IActionResult> RenderTemplate(int id, [FromBody] object data)
    {
        var result = await _templateService.RenderTemplateAsync(id, data);
        return Success(new { RenderedContent = result });
    }
}
