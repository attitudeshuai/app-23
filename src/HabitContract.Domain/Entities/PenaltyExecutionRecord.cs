using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class PenaltyExecutionRecord : BaseEntity<int>
{
    public int PenaltyRuleId { get; set; }

    public int ContractId { get; set; }

    public int UserId { get; set; }

    public int? ContractViolationId { get; set; }

    public PenaltyType PenaltyType { get; set; }

    public PenaltySeverity Severity { get; set; }

    public PenaltyExecutionStatus Status { get; set; } = PenaltyExecutionStatus.Pending;

    public string CalculatedContent { get; set; } = string.Empty;

    public string? Details { get; set; }

    public decimal? FinancialAmount { get; set; }

    public int CreditScoreChange { get; set; } = 0;

    public bool PaymentCompleted { get; set; } = false;

    public DateTime? PaymentDate { get; set; }

    public DateTime? ExecutionDeadline { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? WaivedByUserId { get; set; }

    public string? WaivedReason { get; set; }

    public DateTime? WaivedAt { get; set; }

    public PenaltyRule PenaltyRule { get; set; } = null!;

    public Contract Contract { get; set; } = null!;

    public User User { get; set; } = null!;

    public ContractViolation? ContractViolation { get; set; }
}
