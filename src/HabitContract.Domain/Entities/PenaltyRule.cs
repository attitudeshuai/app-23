using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class PenaltyRule : BaseEntity<int>
{
    public int ContractId { get; set; }

    public PenaltyType PenaltyType { get; set; } = PenaltyType.Custom;

    public PenaltySeverity DefaultSeverity { get; set; } = PenaltySeverity.Medium;

    public string RuleExpression { get; set; } = string.Empty;

    public string? BaseAmount { get; set; }

    public string? EscalationRule { get; set; }

    public bool CreditScoreAffected { get; set; } = true;

    public int CreditScoreImpact { get; set; } = 5;

    public bool PaymentRequired { get; set; } = false;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public Contract Contract { get; set; } = null!;

    public ICollection<PenaltyExecutionRecord> ExecutionRecords { get; set; } = new List<PenaltyExecutionRecord>();
}
