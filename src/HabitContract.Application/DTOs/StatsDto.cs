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
