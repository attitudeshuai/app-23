using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class ReminderRecord : BaseEntity<int>
{
    public int ContractId { get; set; }

    public int UserId { get; set; }

    public int? SettingId { get; set; }

    public DateTime ReminderDate { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public ReminderChannel Channel { get; set; }

    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;

    public DateTime? SentAt { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ContractInfo { get; set; }

    public Contract Contract { get; set; } = null!;

    public User User { get; set; } = null!;

    public ContractReminderSetting? Setting { get; set; }
}
