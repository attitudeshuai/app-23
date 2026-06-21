using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class HabitTemplateVersion : BaseEntity<int>
{
    public int TemplateId { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DefaultFrequency { get; set; } = string.Empty;
    public int DefaultDurationDays { get; set; }
    public string DefaultGoalDescription { get; set; } = string.Empty;
    public string DefaultSupervisorRule { get; set; } = string.Empty;
    public string? DefaultPenaltyDescription { get; set; }
    public string? ChangeLog { get; set; }

    public HabitTemplate Template { get; set; } = null!;
}
