using HabitContract.Domain.Enums;

namespace HabitContract.Application.DTOs;

public class ViolationTypeStatsDto
{
    public ViolationType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public int SevereCount { get; set; }
}

public class ImprovementSuggestionDto
{
    public ViolationType ViolationType { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public int ViolationCount { get; set; }
}

public class StatsOverviewDto
{
    public int TotalUsers { get; set; }
    public int TotalContracts { get; set; }
    public int ActiveContracts { get; set; }
    public int TotalCheckIns { get; set; }
    public int TotalViolations { get; set; }
    public double CompletedRate { get; set; }
    public List<ViolationTypeStatsDto> ViolationTypeBreakdown { get; set; } = new();
}

public class StatsTrendDto
{
    public string Date { get; set; } = string.Empty;
    public int CheckInCount { get; set; }
    public int ViolationCount { get; set; }
    public int NewContractCount { get; set; }
}

public class ContractStatsDto
{
    public int ContractId { get; set; }
    public string ContractName { get; set; } = string.Empty;
    public int TotalCheckIns { get; set; }
    public int NormalCheckIns { get; set; }
    public int MakeUpCheckIns { get; set; }
    public int MissedCheckIns { get; set; }
    public int PendingCheckIns { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double CompletionRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public int CompletedDays { get; set; }
    public int TotalViolations { get; set; }
    public List<ViolationTypeStatsDto> ViolationTypeBreakdown { get; set; } = new();
    public List<ImprovementSuggestionDto> ImprovementSuggestions { get; set; } = new();
}

public class UserStatsDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int TotalContracts { get; set; }
    public int ActiveContracts { get; set; }
    public int CompletedContracts { get; set; }
    public int TotalCheckIns { get; set; }
    public int NormalCheckIns { get; set; }
    public int MakeUpCheckIns { get; set; }
    public int MissedCheckIns { get; set; }
    public int CurrentMaxStreak { get; set; }
    public int LongestMaxStreak { get; set; }
    public double OverallCompletionRate { get; set; }
    public int TotalViolations { get; set; }
    public List<ViolationTypeStatsDto> ViolationTypeBreakdown { get; set; } = new();
    public List<ImprovementSuggestionDto> ImprovementSuggestions { get; set; } = new();
    public List<UserContractStatsDto> ContractStats { get; set; } = new();
}

public class UserContractStatsDto
{
    public int ContractId { get; set; }
    public string ContractName { get; set; } = string.Empty;
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double CompletionRate { get; set; }
}

