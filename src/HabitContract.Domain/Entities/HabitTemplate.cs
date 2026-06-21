using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class HabitTemplate : BaseEntity<int>
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string DefaultFrequency { get; set; } = string.Empty;
    public int DefaultDurationDays { get; set; }
    public string DefaultGoalDescription { get; set; } = string.Empty;
    public string DefaultSupervisorRule { get; set; } = string.Empty;
    public string? DefaultPenaltyDescription { get; set; }
    public string Version { get; set; } = "1.0.0";
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    public int SortOrder { get; set; }
    public int UsageCount { get; set; }
    public int CompletionCount { get; set; }

    public HabitTemplateCategory Category { get; set; } = null!;
}
