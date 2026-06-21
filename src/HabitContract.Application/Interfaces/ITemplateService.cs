using HabitContract.Application.DTOs;
using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.Interfaces;

public interface ITemplateService
{
    Task<List<TemplateCategoryDto>> GetCategoriesAsync();
    Task<List<TemplateDto>> GetTemplatesByCategoryAsync(int categoryId);
    Task<List<TemplateDto>> GetAllPublishedTemplatesAsync();
    Task<TemplateDetailDto> GetTemplateByIdAsync(int id);
    Task<TemplateDraftDto> GenerateDraftAsync(int templateId);
    Task<ContractDto> CreateContractFromTemplateAsync(int userId, CreateContractFromTemplateDto dto);
    Task<List<TemplateDto>> GetRecommendedTemplatesAsync(int userId, int count = 5);

    Task<TemplateCategoryDto> CreateCategoryAsync(TemplateCategoryCreateDto dto);
    Task<TemplateCategoryDto> UpdateCategoryAsync(int id, TemplateCategoryUpdateDto dto);
    Task DeleteCategoryAsync(int id);
    Task<TemplateCategoryDto> GetCategoryByIdAsync(int id);

    Task<TemplateDto> CreateTemplateAsync(TemplateCreateDto dto);
    Task<TemplateDto> UpdateTemplateAsync(int id, TemplateUpdateDto dto);
    Task DeleteTemplateAsync(int id);
    Task<TemplateDto> UpdateTemplateStatusAsync(int id, TemplateStatusDto dto);
    Task<PagedResultDto<TemplateDto>> GetAdminTemplatesAsync(QueryParameters parameters);
    Task<List<TemplateVersionDto>> GetTemplateVersionsAsync(int templateId);
}
