using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class TemplateService : ITemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TemplateService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<TemplateCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _unitOfWork.HabitTemplateCategories.GetActiveCategoriesAsync();
        var result = new List<TemplateCategoryDto>();

        foreach (var category in categories.OrderBy(c => c.SortOrder))
        {
            var dto = _mapper.Map<TemplateCategoryDto>(category);
            dto.TemplateCount = category.Templates.Count(t => t.Status == TemplateStatus.Published);
            result.Add(dto);
        }

        return result;
    }

    public async Task<TemplateCategoryDto> GetCategoryByIdAsync(int id)
    {
        var category = await _unitOfWork.HabitTemplateCategories.GetByIdAsync(id);
        if (category == null)
        {
            throw new BusinessException("分类不存在", 404);
        }

        var dto = _mapper.Map<TemplateCategoryDto>(category);
        dto.TemplateCount = category.Templates.Count;
        return dto;
    }

    public async Task<TemplateCategoryDto> CreateCategoryAsync(TemplateCategoryCreateDto dto)
    {
        var category = _mapper.Map<HabitTemplateCategory>(dto);
        category.IsActive = true;

        var created = await _unitOfWork.HabitTemplateCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TemplateCategoryDto>(created);
    }

    public async Task<TemplateCategoryDto> UpdateCategoryAsync(int id, TemplateCategoryUpdateDto dto)
    {
        var category = await _unitOfWork.HabitTemplateCategories.GetByIdAsync(id);
        if (category == null)
        {
            throw new BusinessException("分类不存在", 404);
        }

        if (!string.IsNullOrEmpty(dto.Name))
            category.Name = dto.Name;

        if (dto.Description != null)
            category.Description = dto.Description;

        if (dto.Icon != null)
            category.Icon = dto.Icon;

        if (dto.SortOrder.HasValue)
            category.SortOrder = dto.SortOrder.Value;

        if (dto.IsActive.HasValue)
            category.IsActive = dto.IsActive.Value;

        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.HabitTemplateCategories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TemplateCategoryDto>(category);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _unitOfWork.HabitTemplateCategories.GetByIdAsync(id);
        if (category == null)
        {
            throw new BusinessException("分类不存在", 404);
        }

        var templates = await _unitOfWork.HabitTemplates.GetByCategoryIdAsync(id);
        if (templates.Any())
        {
            throw new BusinessException("分类下存在模板，无法删除");
        }

        await _unitOfWork.HabitTemplateCategories.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<TemplateDto>> GetTemplatesByCategoryAsync(int categoryId)
    {
        var category = await _unitOfWork.HabitTemplateCategories.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new BusinessException("分类不存在", 404);
        }

        var templates = await _unitOfWork.HabitTemplates.GetPublishedByCategoryIdAsync(categoryId);
        var result = new List<TemplateDto>();

        foreach (var template in templates.OrderBy(t => t.SortOrder))
        {
            var dto = _mapper.Map<TemplateDto>(template);
            dto.CategoryName = category.Name;
            dto.CompletionRate = template.UsageCount > 0
                ? Math.Round((double)template.CompletionCount / template.UsageCount * 100, 1)
                : 0;
            result.Add(dto);
        }

        return result;
    }

    public async Task<List<TemplateDto>> GetAllPublishedTemplatesAsync()
    {
        var templates = await _unitOfWork.HabitTemplates.GetPublishedTemplatesAsync();
        var result = new List<TemplateDto>();

        foreach (var template in templates.OrderBy(t => t.SortOrder))
        {
            var dto = _mapper.Map<TemplateDto>(template);
            if (template.Category != null)
            {
                dto.CategoryName = template.Category.Name;
            }
            dto.CompletionRate = template.UsageCount > 0
                ? Math.Round((double)template.CompletionCount / template.UsageCount * 100, 1)
                : 0;
            result.Add(dto);
        }

        return result;
    }

    public async Task<TemplateDetailDto> GetTemplateByIdAsync(int id)
    {
        var template = await _unitOfWork.HabitTemplates.GetByIdAsync(id);
        if (template == null)
        {
            throw new BusinessException("模板不存在", 404);
        }

        if (template.Status != TemplateStatus.Published)
        {
            throw new BusinessException("模板未发布", 403);
        }

        var dto = _mapper.Map<TemplateDetailDto>(template);
        if (template.Category != null)
        {
            dto.CategoryName = template.Category.Name;
        }
        dto.CompletionRate = template.UsageCount > 0
            ? Math.Round((double)template.CompletionCount / template.UsageCount * 100, 1)
            : 0;

        return dto;
    }

    public async Task<TemplateDraftDto> GenerateDraftAsync(int templateId)
    {
        var template = await _unitOfWork.HabitTemplates.GetByIdAsync(templateId);
        if (template == null)
        {
            throw new BusinessException("模板不存在", 404);
        }

        if (template.Status != TemplateStatus.Published)
        {
            throw new BusinessException("模板未发布", 403);
        }

        var today = DateTime.UtcNow.Date;
        var endDate = today.AddDays(template.DefaultDurationDays);

        return new TemplateDraftDto
        {
            TemplateId = template.Id,
            TemplateName = template.Name,
            HabitName = template.Name,
            Frequency = template.DefaultFrequency,
            StartDate = today,
            EndDate = endDate,
            GoalDescription = template.DefaultGoalDescription,
            SupervisorRule = template.DefaultSupervisorRule,
            PenaltyDescription = template.DefaultPenaltyDescription
        };
    }

    public async Task<ContractDto> CreateContractFromTemplateAsync(int userId, CreateContractFromTemplateDto dto)
    {
        var template = await _unitOfWork.HabitTemplates.GetByIdAsync(dto.TemplateId);
        if (template == null)
        {
            throw new BusinessException("模板不存在", 404);
        }

        if (template.Status != TemplateStatus.Published)
        {
            throw new BusinessException("模板未发布", 403);
        }

        if (dto.EndDate <= dto.StartDate)
        {
            throw new BusinessException("结束日期必须大于开始日期");
        }

        var contract = new Contract
        {
            OwnerId = userId,
            TemplateId = dto.TemplateId,
            HabitName = dto.HabitName,
            Frequency = dto.Frequency,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            PenaltyDescription = dto.PenaltyDescription,
            Status = ContractStatus.Active
        };

        var created = await _unitOfWork.Contracts.AddAsync(contract);
        template.UsageCount++;
        template.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.HabitTemplates.UpdateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        var contractDto = _mapper.Map<ContractDto>(created);
        var owner = await _unitOfWork.Users.GetByIdAsync(userId);
        contractDto.OwnerName = owner?.Username;

        return contractDto;
    }

    public async Task<List<TemplateDto>> GetRecommendedTemplatesAsync(int userId, int count = 5)
    {
        var allTemplates = await _unitOfWork.HabitTemplates.GetPublishedTemplatesAsync();
        var allContracts = await _unitOfWork.Contracts.GetAllAsync();
        var userContracts = allContracts.Where(c => c.OwnerId == userId).ToList();

        var completedTemplateIds = new HashSet<int>();
        var failedTemplateIds = new HashSet<int>();
        var usedTemplateIds = new HashSet<int>();

        foreach (var contract in userContracts)
        {
            if (contract.TemplateId.HasValue)
            {
                usedTemplateIds.Add(contract.TemplateId.Value);
                if (contract.Status == ContractStatus.Completed)
                {
                    completedTemplateIds.Add(contract.TemplateId.Value);
                }
                else if (contract.Status == ContractStatus.Failed)
                {
                    failedTemplateIds.Add(contract.TemplateId.Value);
                }
            }
        }

        var scoredTemplates = allTemplates
            .Select(t => new
            {
                Template = t,
                Score = CalculateRecommendationScore(t, completedTemplateIds, failedTemplateIds, usedTemplateIds)
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .ToList();

        var result = new List<TemplateDto>();
        foreach (var item in scoredTemplates)
        {
            var dto = _mapper.Map<TemplateDto>(item.Template);
            if (item.Template.Category != null)
            {
                dto.CategoryName = item.Template.Category.Name;
            }
            dto.CompletionRate = item.Template.UsageCount > 0
                ? Math.Round((double)item.Template.CompletionCount / item.Template.UsageCount * 100, 1)
                : 0;
            result.Add(dto);
        }

        return result;
    }

    private static double CalculateRecommendationScore(HabitTemplate template, HashSet<int> completedTemplates, HashSet<int> failedTemplates, HashSet<int> usedTemplates)
    {
        double score = 0;

        if (template.UsageCount > 0)
        {
            var completionRate = (double)template.CompletionCount / template.UsageCount;
            score += completionRate * 50;
        }

        score += Math.Min(template.UsageCount, 100) * 0.3;

        if (completedTemplates.Contains(template.Id))
        {
            score -= 25;
        }
        else if (usedTemplates.Contains(template.Id))
        {
            score -= 10;
        }

        if (failedTemplates.Contains(template.Id))
        {
            score -= 15;
        }

        return score;
    }

    public async Task<TemplateDto> CreateTemplateAsync(TemplateCreateDto dto)
    {
        var category = await _unitOfWork.HabitTemplateCategories.GetByIdAsync(dto.CategoryId);
        if (category == null)
        {
            throw new BusinessException("分类不存在", 404);
        }

        var template = _mapper.Map<HabitTemplate>(dto);
        template.Version = "1.0.0";
        template.Status = TemplateStatus.Draft;
        template.UsageCount = 0;
        template.CompletionCount = 0;

        var created = await _unitOfWork.HabitTemplates.AddAsync(template);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<TemplateDto>(created);
        result.CategoryName = category.Name;
        return result;
    }

    public async Task<TemplateDto> UpdateTemplateAsync(int id, TemplateUpdateDto dto)
    {
        var template = await _unitOfWork.HabitTemplates.GetByIdAsync(id);
        if (template == null)
        {
            throw new BusinessException("模板不存在", 404);
        }

        var hasChanges = false;

        if (dto.CategoryId.HasValue && dto.CategoryId.Value != template.CategoryId)
        {
            var category = await _unitOfWork.HabitTemplateCategories.GetByIdAsync(dto.CategoryId.Value);
            if (category == null)
            {
                throw new BusinessException("分类不存在", 404);
            }
            template.CategoryId = dto.CategoryId.Value;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(dto.Name) && dto.Name != template.Name)
        {
            template.Name = dto.Name;
            hasChanges = true;
        }

        if (dto.Description != null && dto.Description != template.Description)
        {
            template.Description = dto.Description;
            hasChanges = true;
        }

        if (dto.Icon != null && dto.Icon != template.Icon)
        {
            template.Icon = dto.Icon;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(dto.DefaultFrequency) && dto.DefaultFrequency != template.DefaultFrequency)
        {
            template.DefaultFrequency = dto.DefaultFrequency;
            hasChanges = true;
        }

        if (dto.DefaultDurationDays.HasValue && dto.DefaultDurationDays.Value != template.DefaultDurationDays)
        {
            template.DefaultDurationDays = dto.DefaultDurationDays.Value;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(dto.DefaultGoalDescription) && dto.DefaultGoalDescription != template.DefaultGoalDescription)
        {
            template.DefaultGoalDescription = dto.DefaultGoalDescription;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(dto.DefaultSupervisorRule) && dto.DefaultSupervisorRule != template.DefaultSupervisorRule)
        {
            template.DefaultSupervisorRule = dto.DefaultSupervisorRule;
            hasChanges = true;
        }

        if (dto.DefaultPenaltyDescription != null && dto.DefaultPenaltyDescription != template.DefaultPenaltyDescription)
        {
            template.DefaultPenaltyDescription = dto.DefaultPenaltyDescription;
            hasChanges = true;
        }

        if (dto.SortOrder.HasValue && dto.SortOrder.Value != template.SortOrder)
        {
            template.SortOrder = dto.SortOrder.Value;
            hasChanges = true;
        }

        if (hasChanges)
        {
            var versionParts = template.Version.Split('.');
            if (versionParts.Length >= 3 && int.TryParse(versionParts[2], out int patch))
            {
                patch++;
                template.Version = $"{versionParts[0]}.{versionParts[1]}.{patch}";
            }
            else
            {
                template.Version = "1.0.1";
            }

            var versionRecord = new HabitTemplateVersion
            {
                TemplateId = template.Id,
                Version = template.Version,
                Name = template.Name,
                Description = template.Description,
                DefaultFrequency = template.DefaultFrequency,
                DefaultDurationDays = template.DefaultDurationDays,
                DefaultGoalDescription = template.DefaultGoalDescription,
                DefaultSupervisorRule = template.DefaultSupervisorRule,
                DefaultPenaltyDescription = template.DefaultPenaltyDescription,
                ChangeLog = dto.ChangeLog
            };

            await _unitOfWork.HabitTemplateVersions.AddAsync(versionRecord);

            template.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.HabitTemplates.UpdateAsync(template);
            await _unitOfWork.SaveChangesAsync();
        }

        var result = _mapper.Map<TemplateDto>(template);
        if (template.Category != null)
        {
            result.CategoryName = template.Category.Name;
        }
        result.CompletionRate = template.UsageCount > 0
            ? Math.Round((double)template.CompletionCount / template.UsageCount * 100, 1)
            : 0;

        return result;
    }

    public async Task DeleteTemplateAsync(int id)
    {
        var template = await _unitOfWork.HabitTemplates.GetByIdAsync(id);
        if (template == null)
        {
            throw new BusinessException("模板不存在", 404);
        }

        if (template.UsageCount > 0)
        {
            throw new BusinessException("模板已被使用，无法删除，请下线处理");
        }

        await _unitOfWork.HabitTemplates.DeleteAsync(template);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<TemplateDto> UpdateTemplateStatusAsync(int id, TemplateStatusDto dto)
    {
        var template = await _unitOfWork.HabitTemplates.GetByIdAsync(id);
        if (template == null)
        {
            throw new BusinessException("模板不存在", 404);
        }

        template.Status = dto.Status;
        template.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.HabitTemplates.UpdateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<TemplateDto>(template);
        if (template.Category != null)
        {
            result.CategoryName = template.Category.Name;
        }
        result.CompletionRate = template.UsageCount > 0
            ? Math.Round((double)template.CompletionCount / template.UsageCount * 100, 1)
            : 0;

        return result;
    }

    public async Task<PagedResultDto<TemplateDto>> GetAdminTemplatesAsync(QueryParameters parameters)
    {
        var pagedResult = await _unitOfWork.HabitTemplates.GetPagedAsync(parameters);
        var items = new List<TemplateDto>();

        foreach (var template in pagedResult.Items)
        {
            var dto = _mapper.Map<TemplateDto>(template);
            if (template.Category != null)
            {
                dto.CategoryName = template.Category.Name;
            }
            dto.CompletionRate = template.UsageCount > 0
                ? Math.Round((double)template.CompletionCount / template.UsageCount * 100, 1)
                : 0;
            items.Add(dto);
        }

        return new PagedResultDto<TemplateDto>
        {
            Items = items,
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalPages = pagedResult.TotalPages,
            HasPreviousPage = pagedResult.HasPreviousPage,
            HasNextPage = pagedResult.HasNextPage
        };
    }

    public async Task<List<TemplateVersionDto>> GetTemplateVersionsAsync(int templateId)
    {
        var template = await _unitOfWork.HabitTemplates.GetByIdAsync(templateId);
        if (template == null)
        {
            throw new BusinessException("模板不存在", 404);
        }

        var versions = await _unitOfWork.HabitTemplateVersions.GetByTemplateIdAsync(templateId);
        return versions
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => _mapper.Map<TemplateVersionDto>(v))
            .ToList();
    }
}
