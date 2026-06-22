namespace HabitContract.Domain.Enums;

public enum ViolationType
{
    MissedCheckIn = 0,
    NotMetTarget = 1,

    [Obsolete("历史枚举值，请使用 NotMetTarget。数据迁移时自动将 Unapproved 转为 NotMetTarget。")]
    Unapproved = 2,

    Other = 3
}

public static class ViolationTypeMigrator
{
    public static ViolationType Normalize(ViolationType type)
    {
        return type switch
        {
            ViolationType.Unapproved => ViolationType.NotMetTarget,
            _ => type
        };
    }

    public static bool IsObsolete(ViolationType type)
    {
        return type == ViolationType.Unapproved;
    }
}
