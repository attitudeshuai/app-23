using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;

namespace HabitContract.Api.Controllers;

[ApiController]
[Route("api/templates")]
public class TemplatesController : ApiControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplatesController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _templateService.GetCategoriesAsync();
        return Success(result);
    }

    [HttpGet("categories/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var result = await _templateService.GetCategoryByIdAsync(id);
        return Success(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublishedTemplates()
    {
        var result = await _templateService.GetAllPublishedTemplatesAsync();
        return Success(result);
    }

    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTemplatesByCategory(int categoryId)
    {
        var result = await _templateService.GetTemplatesByCategoryAsync(categoryId);
        return Success(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTemplateById(int id)
    {
        var result = await _templateService.GetTemplateByIdAsync(id);
        return Success(result);
    }

    [HttpGet("{templateId}/draft")]
    [Authorize]
    public async Task<IActionResult> GenerateDraft(int templateId)
    {
        var result = await _templateService.GenerateDraftAsync(templateId);
        return Success(result);
    }

    [HttpPost("create-contract")]
    [Authorize]
    public async Task<IActionResult> CreateContractFromTemplate([FromBody] CreateContractFromTemplateDto dto)
    {
        var result = await _templateService.CreateContractFromTemplateAsync(GetCurrentUserId(), dto);
        return Success(result, "创建成功");
    }

    [HttpGet("recommendations")]
    [Authorize]
    public async Task<IActionResult> GetRecommendedTemplates([FromQuery] int count = 5)
    {
        var result = await _templateService.GetRecommendedTemplatesAsync(GetCurrentUserId(), count);
        return Success(result);
    }

    [HttpPost("categories")]
    [Authorize]
    public async Task<IActionResult> CreateCategory([FromBody] TemplateCategoryCreateDto dto)
    {
        var result = await _templateService.CreateCategoryAsync(dto);
        return Success(result, "创建成功");
    }

    [HttpPut("categories/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] TemplateCategoryUpdateDto dto)
    {
        var result = await _templateService.UpdateCategoryAsync(id, dto);
        return Success(result, "更新成功");
    }

    [HttpDelete("categories/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _templateService.DeleteCategoryAsync(id);
        return Success("删除成功");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateTemplate([FromBody] TemplateCreateDto dto)
    {
        var result = await _templateService.CreateTemplateAsync(dto);
        return Success(result, "创建成功");
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTemplate(int id, [FromBody] TemplateUpdateDto dto)
    {
        var result = await _templateService.UpdateTemplateAsync(id, dto);
        return Success(result, "更新成功");
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTemplate(int id)
    {
        await _templateService.DeleteTemplateAsync(id);
        return Success("删除成功");
    }

    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateTemplateStatus(int id, [FromBody] TemplateStatusDto dto)
    {
        var result = await _templateService.UpdateTemplateStatusAsync(id, dto);
        return Success(result, "状态更新成功");
    }

    [HttpGet("admin")]
    [Authorize]
    public async Task<IActionResult> GetAdminTemplates([FromQuery] QueryParameters parameters)
    {
        var result = await _templateService.GetAdminTemplatesAsync(parameters);
        return Success(result);
    }

    [HttpGet("{templateId}/versions")]
    [Authorize]
    public async Task<IActionResult> GetTemplateVersions(int templateId)
    {
        var result = await _templateService.GetTemplateVersionsAsync(templateId);
        return Success(result);
    }
}
