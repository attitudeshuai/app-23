namespace HabitContract.Application.DTOs;

public class StatsOverviewDto
{
    public int TotalUsers { get; set; }
    public int TotalContracts { get; set; }
    public int ActiveContracts { get; set; }
    public int TotalCheckIns { get; set; }
    public int TotalViolations { get; set; }
    public double CompletedRate { get; set; }
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
    public List<UserContractStatsDto> ContractStats { get; set; } = new List<UserContractStatsDto>();
}

public class UserContractStatsDto
{
    public int ContractId { get; set; }
    public string ContractName { get; set; } = string.Empty;
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double CompletionRate { get; set; }
}

