namespace HabitContract.Domain.Enums;

public enum ReminderChannel
{
    InApp = 1,
    Email = 2,
    Sms = 3,
    Push = 4
}

public enum ReminderStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Cancelled = 3
}

public enum ReminderTemplateType
{
    DefaultCheckInReminder = 1,
    DailySummary = 2,
    ViolationWarning = 3
}
