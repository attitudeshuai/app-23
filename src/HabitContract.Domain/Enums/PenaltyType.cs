namespace HabitContract.Domain.Enums;

public enum PenaltyType
{
    Financial = 0,
    Service = 1,
    Social = 2,
    Physical = 3,
    Custom = 4
}

public enum PenaltyExecutionStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Waived = 3,
    Failed = 4
}

public enum PenaltySeverity
{
    Light = 0,
    Medium = 1,
    Severe = 2
}

public static class PenaltyTypeMigrator
{
    public static string GetPenaltyTypeName(PenaltyType type)
    {
        return type switch
        {
            PenaltyType.Financial => "经济惩罚",
            PenaltyType.Service => "服务惩罚",
            PenaltyType.Social => "社交惩罚",
            PenaltyType.Physical => "体力惩罚",
            PenaltyType.Custom => "自定义惩罚",
            _ => "未知类型"
        };
    }

    public static string GetExecutionStatusName(PenaltyExecutionStatus status)
    {
        return status switch
        {
            PenaltyExecutionStatus.Pending => "待执行",
            PenaltyExecutionStatus.InProgress => "执行中",
            PenaltyExecutionStatus.Completed => "已完成",
            PenaltyExecutionStatus.Waived => "已豁免",
            PenaltyExecutionStatus.Failed => "执行失败",
            _ => "未知状态"
        };
    }

    public static string GetSeverityName(PenaltySeverity severity)
    {
        return severity switch
        {
            PenaltySeverity.Light => "轻度",
            PenaltySeverity.Medium => "中度",
            PenaltySeverity.Severe => "重度",
            _ => "未知级别"
        };
    }
}
