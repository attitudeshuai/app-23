using System.ComponentModel.DataAnnotations;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.DTOs;

public class ContractCreateDto
{
    [Required(ErrorMessage = "习惯名称不能为空")]
    [StringLength(100, ErrorMessage = "习惯名称最多100个字符")]
    public string HabitName { get; set; } = string.Empty;

    [Required(ErrorMessage = "频率不能为空")]
    [StringLength(50, ErrorMessage = "频率最多50个字符")]
    public string Frequency { get; set; } = string.Empty;

    [Required(ErrorMessage = "开始日期不能为空")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "结束日期不能为空")]
    public DateTime EndDate { get; set; }

    [StringLength(500, ErrorMessage = "惩罚描述最多500个字符")]
    public string? PenaltyDescription { get; set; }

    public TimeSpan CheckInDeadline { get; set; } = new TimeSpan(23, 59, 59);

    [StringLength(50, ErrorMessage = "时区最多50个字符")]
    public string TimeZone { get; set; } = "Asia/Shanghai";

    public int MakeUpDeadlineDays { get; set; } = 7;
}

public class ContractUpdateDto
{
    [StringLength(100, ErrorMessage = "习惯名称最多100个字符")]
    public string? HabitName { get; set; }

    [StringLength(50, ErrorMessage = "频率最多50个字符")]
    public string? Frequency { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [StringLength(500, ErrorMessage = "惩罚描述最多500个字符")]
    public string? PenaltyDescription { get; set; }

    public TimeSpan? CheckInDeadline { get; set; }

    [StringLength(50, ErrorMessage = "时区最多50个字符")]
    public string? TimeZone { get; set; }

    public int? MakeUpDeadlineDays { get; set; }
}

public class ContractStatusDto
{
    [Required(ErrorMessage = "状态不能为空")]
    public ContractStatus Status { get; set; }
}

public class ContractDto
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string HabitName { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? PenaltyDescription { get; set; }
    public ContractStatus Status { get; set; }
    public TimeSpan CheckInDeadline { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public int MakeUpDeadlineDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int PartnerCount { get; set; }
    public int CheckInCount { get; set; }
    public int ViolationCount { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
}

public class ContractListDto
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string HabitName { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public ContractStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int CurrentStreak { get; set; }
    public DateTime CreatedAt { get; set; }
}
