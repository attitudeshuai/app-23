using HabitContract.Domain.Common;

namespace HabitContract.Domain.Entities;

public class HabitTemplateCategory : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<HabitTemplate> Templates { get; set; } = new List<HabitTemplate>();
}
