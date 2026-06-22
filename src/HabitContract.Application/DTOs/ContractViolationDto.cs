using System.ComponentModel.DataAnnotations;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.DTOs;

public class ContractViolationCreateDto
{
    [Required(ErrorMessage = "契约ID不能为空")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "违约用户ID不能为空")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "违约日期不能为空")]
    public DateTime ViolationDate { get; set; }

    [Required(ErrorMessage = "违约类型不能为空")]
    public ViolationType ViolationType { get; set; }

    public bool IsSevere { get; set; } = false;

    [Required(ErrorMessage = "违约原因不能为空")]
    [StringLength(500, ErrorMessage = "违约原因最多500个字符")]
    public string Reason { get; set; } = string.Empty;

    public bool AutoCalculatePenalty { get; set; } = true;
}

public class ContractViolationUpdateDto
{
    public ViolationType? ViolationType { get; set; }

    public bool? IsSevere { get; set; }

    [StringLength(500, ErrorMessage = "违约原因最多500个字符")]
    public string? Reason { get; set; }

    public bool? IsConfirmed { get; set; }
}

public class ContractViolationDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public DateTime ViolationDate { get; set; }
    public ViolationType ViolationType { get; set; }
    public bool IsSevere { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PenaltyExecutionDto> PenaltyExecutionRecords { get; set; } = new();
    public PenaltyCalculationResult? PenaltyCalculationResult { get; set; }
}
