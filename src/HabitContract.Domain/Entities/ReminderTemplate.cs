using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class ReminderTemplate : BaseEntity<int>
{
    public ReminderTemplateType Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TitleTemplate { get; set; } = string.Empty;

    public string ContentTemplate { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDefault { get; set; } = false;
}
