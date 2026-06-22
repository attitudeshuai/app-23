using System.ComponentModel.DataAnnotations;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.DTOs;

public class ReviewContextDto
{
    public int TotalCheckIns { get; set; }
    public int NormalCheckIns { get; set; }
    public int MakeUpCheckIns { get; set; }
    public int MissedCheckIns { get; set; }
    public double CompletionRate { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int Recent30DaysViolations { get; set; }
    public int Recent30DaysMakeUpRequests { get; set; }
    public List<string>? RecentCheckInDates { get; set; }
}

public class MakeUpRequestCreateDto
{
    [Required(ErrorMessage = "契约ID不能为空")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "补卡日期不能为空")]
    public DateTime CheckInDate { get; set; }

    [Required(ErrorMessage = "补卡原因不能为空")]
    [StringLength(500, ErrorMessage = "补卡原因最多500个字符")]
    public string Reason { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "打卡证明文字最多1000个字符")]
    public string? ProofText { get; set; }

    [StringLength(500, ErrorMessage = "打卡证明图片链接最多500个字符")]
    public string? ProofPhoto { get; set; }
}

public class MakeUpRequestReviewDto
{
    [Required(ErrorMessage = "审核结果不能为空")]
    public MakeUpRequestStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "拒绝原因最多500个字符")]
    public string? RejectionReason { get; set; }
}

public class MakeUpRequestDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public DateTime CheckInDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ProofText { get; set; }
    public string? ProofPhoto { get; set; }
    public MakeUpRequestStatus Status { get; set; }
    public int? ReviewedBy { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public ReviewContextDto? ReviewContext { get; set; }
}

public class MakeUpRequestListDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public DateTime CheckInDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public MakeUpRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
