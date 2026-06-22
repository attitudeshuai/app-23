using System.ComponentModel.DataAnnotations;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.DTOs;

public class PenaltyRuleCreateDto
{
    [Required(ErrorMessage = "契约ID不能为空")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "惩罚类型不能为空")]
    public PenaltyType PenaltyType { get; set; } = PenaltyType.Custom;

    public PenaltySeverity DefaultSeverity { get; set; } = PenaltySeverity.Medium;

    [Required(ErrorMessage = "规则表达式不能为空")]
    [StringLength(1000, ErrorMessage = "规则表达式最多1000个字符")]
    public string RuleExpression { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "基础数值最多200个字符")]
    public string? BaseAmount { get; set; }

    [StringLength(500, ErrorMessage = "升级规则最多500个字符")]
    public string? EscalationRule { get; set; }

    public bool CreditScoreAffected { get; set; } = true;

    public int CreditScoreImpact { get; set; } = 5;

    public bool PaymentRequired { get; set; } = false;

    [StringLength(500, ErrorMessage = "描述最多500个字符")]
    public string? Description { get; set; }
}

public class PenaltyRuleUpdateDto
{
    public PenaltyType? PenaltyType { get; set; }

    public PenaltySeverity? DefaultSeverity { get; set; }

    [StringLength(1000, ErrorMessage = "规则表达式最多1000个字符")]
    public string? RuleExpression { get; set; }

    [StringLength(200, ErrorMessage = "基础数值最多200个字符")]
    public string? BaseAmount { get; set; }

    [StringLength(500, ErrorMessage = "升级规则最多500个字符")]
    public string? EscalationRule { get; set; }

    public bool? CreditScoreAffected { get; set; }

    public int? CreditScoreImpact { get; set; }

    public bool? PaymentRequired { get; set; }

    [StringLength(500, ErrorMessage = "描述最多500个字符")]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}

public class PenaltyRuleDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public PenaltyType PenaltyType { get; set; }
    public string PenaltyTypeName { get; set; } = string.Empty;
    public PenaltySeverity DefaultSeverity { get; set; }
    public string DefaultSeverityName { get; set; } = string.Empty;
    public string RuleExpression { get; set; } = string.Empty;
    public string? BaseAmount { get; set; }
    public string? EscalationRule { get; set; }
    public bool CreditScoreAffected { get; set; }
    public int CreditScoreImpact { get; set; }
    public bool PaymentRequired { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PenaltyExecutionCreateDto
{
    [Required(ErrorMessage = "契约ID不能为空")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "用户ID不能为空")]
    public int UserId { get; set; }

    public int? ContractViolationId { get; set; }
}

public class PenaltyExecutionUpdateDto
{
    public PenaltyExecutionStatus? Status { get; set; }

    [StringLength(1000, ErrorMessage = "详情最多1000个字符")]
    public string? Details { get; set; }

    public bool? PaymentCompleted { get; set; }

    public DateTime? PaymentDate { get; set; }

    public DateTime? CompletedAt { get; set; }
}

public class PenaltyExecutionWaiveDto
{
    [Required(ErrorMessage = "豁免原因不能为空")]
    [StringLength(500, ErrorMessage = "豁免原因最多500个字符")]
    public string WaivedReason { get; set; } = string.Empty;
}

public class PenaltyExecutionDto
{
    public int Id { get; set; }
    public int PenaltyRuleId { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public int? ContractViolationId { get; set; }
    public PenaltyType PenaltyType { get; set; }
    public string PenaltyTypeName { get; set; } = string.Empty;
    public PenaltySeverity Severity { get; set; }
    public string SeverityName { get; set; } = string.Empty;
    public PenaltyExecutionStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string CalculatedContent { get; set; } = string.Empty;
    public string? Details { get; set; }
    public decimal? FinancialAmount { get; set; }
    public int CreditScoreChange { get; set; }
    public bool PaymentCompleted { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? ExecutionDeadline { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? WaivedByUserId { get; set; }
    public string? WaivedByUsername { get; set; }
    public string? WaivedReason { get; set; }
    public DateTime? WaivedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PenaltyTrendDto
{
    public string Date { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
    public int PendingCount { get; set; }
    public int WaivedCount { get; set; }
    public decimal TotalFinancialAmount { get; set; }
}

public class PenaltyOverviewDto
{
    public int TotalRecords { get; set; }
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int WaivedCount { get; set; }
    public int FailedCount { get; set; }
    public decimal TotalFinancialAmount { get; set; }
    public decimal PaidFinancialAmount { get; set; }
    public decimal UnpaidFinancialAmount { get; set; }
    public int TotalCreditScoreImpact { get; set; }
}

public class PenaltyCalculationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PenaltyType PenaltyType { get; set; }
    public PenaltySeverity Severity { get; set; }
    public string CalculatedContent { get; set; } = string.Empty;
    public decimal? FinancialAmount { get; set; }
    public int CreditScoreChange { get; set; }
    public bool PaymentRequired { get; set; }
    public bool UsesDefaultRule { get; set; }
    public bool RequiresAdminConfiguration { get; set; }
}

public class DefaultPenaltyConfigDto
{
    public int ContractId { get; set; }
    public string ContractName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool RequiresConfiguration { get; set; }
    public PenaltyRuleDto? SuggestedRule { get; set; }
}
