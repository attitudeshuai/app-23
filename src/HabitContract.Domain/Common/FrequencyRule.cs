using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Common;

public class FrequencyRule
{
    public FrequencyType Type { get; set; }
    public int Count { get; set; }
    public List<DayOfWeek>? DaysOfWeek { get; set; }
    public string OriginalString { get; set; } = string.Empty;
}

public class FrequencyValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int CurrentCount { get; set; }
    public int RequiredCount { get; set; }
    public string Period { get; set; } = string.Empty;
    public ViolationType ViolationType { get; set; } = ViolationType.Other;
}
