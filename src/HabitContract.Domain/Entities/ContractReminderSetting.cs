using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class ContractReminderSetting : BaseEntity<int>
{
    public int ContractId { get; set; }

    public int UserId { get; set; }

    public TimeSpan ReminderTime { get; set; }

    public bool IsEnabled { get; set; } = true;

    public TimeSpan? QuietStart { get; set; }

    public TimeSpan? QuietEnd { get; set; }

    public ReminderChannel Channel { get; set; } = ReminderChannel.InApp;

    public Contract Contract { get; set; } = null!;

    public User User { get; set; } = null!;

    public ICollection<ReminderRecord> ReminderRecords { get; set; } = new List<ReminderRecord>();
}
