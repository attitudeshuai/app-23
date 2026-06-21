using HabitContract.Application.DTOs;

namespace HabitContract.Application.Interfaces;

public interface IReminderTemplateService
{
    Task<IEnumerable<ReminderTemplateDto>> GetAllTemplatesAsync();

    Task<ReminderTemplateDto> GetTemplateByIdAsync(int id);

    Task<ReminderTemplateDto> CreateTemplateAsync(ReminderTemplateCreateDto dto);

    Task<ReminderTemplateDto> UpdateTemplateAsync(int id, ReminderTemplateUpdateDto dto);

    Task DeleteTemplateAsync(int id);

    Task<string> RenderTemplateAsync(int templateId, object data);
}
