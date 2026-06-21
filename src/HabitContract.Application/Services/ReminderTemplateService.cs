using System.Text.RegularExpressions;
using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class ReminderTemplateService : IReminderTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReminderTemplateService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ReminderTemplateDto>> GetAllTemplatesAsync()
    {
        var templates = await _unitOfWork.ReminderTemplates.GetAllAsync();
        return _mapper.Map<IEnumerable<ReminderTemplateDto>>(templates);
    }

    public async Task<ReminderTemplateDto> GetTemplateByIdAsync(int id)
    {
        var template = await _unitOfWork.ReminderTemplates.GetByIdAsync(id);
        if (template == null)
        {
            throw new BusinessException("模板不存在", 404);
        }

        return _mapper.Map<ReminderTemplateDto>(template);
    }

    public async Task<ReminderTemplateDto> CreateTemplateAsync(ReminderTemplateCreateDto dto)
    {
        var existingDefault = await _unitOfWork.ReminderTemplates.GetDefaultAsync(dto.Type);
        if (dto.IsDefault && existingDefault != null)
        {
            existingDefault.IsDefault = false;
            existingDefault.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.ReminderTemplates.UpdateAsync(existingDefault);
        }

        var template = _mapper.Map<ReminderTemplate>(dto);
        template.CreatedAt = DateTime.UtcNow;

        if (dto.IsDefault)
        {
            template.IsDefault = true;
        }

        var created = await _unitOfWork.ReminderTemplates.AddAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ReminderTemplateDto>(created);
    }

    public async Task<ReminderTemplateDto> UpdateTemplateAsync(int id, ReminderTemplateUpdateDto dto)
    {
        var template = await _unitOfWork.ReminderTemplates.GetByIdAsync(id);
        if (template == null)
        {
            throw new BusinessException("模板不存在", 404);
        }

        if (dto.IsDefault.HasValue && dto.IsDefault.Value)
        {
            var existingDefault = await _unitOfWork.ReminderTemplates.GetDefaultAsync(template.Type);
            if (existingDefault != null && existingDefault.Id != id)
            {
                existingDefault.IsDefault = false;
                existingDefault.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.ReminderTemplates.UpdateAsync(existingDefault);
            }
        }

        if (dto.Name != null)
            template.Name = dto.Name;
        if (dto.TitleTemplate != null)
            template.TitleTemplate = dto.TitleTemplate;
        if (dto.ContentTemplate != null)
            template.ContentTemplate = dto.ContentTemplate;
        if (dto.Description != null)
            template.Description = dto.Description;
        if (dto.IsActive.HasValue)
            template.IsActive = dto.IsActive.Value;
        if (dto.IsDefault.HasValue)
            template.IsDefault = dto.IsDefault.Value;

        template.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ReminderTemplates.UpdateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ReminderTemplateDto>(template);
    }

    public async Task DeleteTemplateAsync(int id)
    {
        var template = await _unitOfWork.ReminderTemplates.GetByIdAsync(id);
        if (template == null)
        {
            throw new BusinessException("模板不存在", 404);
        }

        if (template.IsDefault)
        {
            throw new BusinessException("无法删除默认模板");
        }

        await _unitOfWork.ReminderTemplates.DeleteAsync(template);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<string> RenderTemplateAsync(int templateId, object data)
    {
        var template = await _unitOfWork.ReminderTemplates.GetByIdAsync(templateId);
        if (template == null)
        {
            throw new BusinessException("模板不存在", 404);
        }

        return RenderContent(template.ContentTemplate, data);
    }

    private string RenderContent(string template, object data)
    {
        var result = template;
        var properties = data.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var pattern = $"{{{prop.Name}}}";
            var value = prop.GetValue(data)?.ToString() ?? string.Empty;
            result = Regex.Replace(result, pattern, value, RegexOptions.IgnoreCase);
        }

        return result;
    }
}
