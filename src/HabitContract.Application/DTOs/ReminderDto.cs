using System.ComponentModel.DataAnnotations;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.DTOs;

public class ReminderSettingCreateDto
{
    [Required(ErrorMessage = "契约ID不能为空")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "提醒时间不能为空")]
    public TimeSpan ReminderTime { get; set; }

    public bool IsEnabled { get; set; } = true;

    public TimeSpan? QuietStart { get; set; }

    public TimeSpan? QuietEnd { get; set; }

    public ReminderChannel Channel { get; set; } = ReminderChannel.InApp;
}

public class ReminderSettingUpdateDto
{
    public TimeSpan? ReminderTime { get; set; }

    public bool? IsEnabled { get; set; }

    public TimeSpan? QuietStart { get; set; }

    public TimeSpan? QuietEnd { get; set; }

    public ReminderChannel? Channel { get; set; }
}

public class ReminderSettingDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public TimeSpan ReminderTime { get; set; }
    public bool IsEnabled { get; set; }
    public TimeSpan? QuietStart { get; set; }
    public TimeSpan? QuietEnd { get; set; }
    public ReminderChannel Channel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ReminderRecordDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public int? SettingId { get; set; }
    public DateTime ReminderDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public ReminderChannel Channel { get; set; }
    public ReminderStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ContractInfo { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReminderTemplateCreateDto
{
    [Required(ErrorMessage = "模板类型不能为空")]
    public ReminderTemplateType Type { get; set; }

    [Required(ErrorMessage = "模板名称不能为空")]
    [StringLength(100, ErrorMessage = "模板名称最多100个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "标题模板不能为空")]
    [StringLength(200, ErrorMessage = "标题模板最多200个字符")]
    public string TitleTemplate { get; set; } = string.Empty;

    [Required(ErrorMessage = "内容模板不能为空")]
    [StringLength(1000, ErrorMessage = "内容模板最多1000个字符")]
    public string ContentTemplate { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "描述最多500个字符")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDefault { get; set; } = false;
}

public class ReminderTemplateUpdateDto
{
    [StringLength(100, ErrorMessage = "模板名称最多100个字符")]
    public string? Name { get; set; }

    [StringLength(200, ErrorMessage = "标题模板最多200个字符")]
    public string? TitleTemplate { get; set; }

    [StringLength(1000, ErrorMessage = "内容模板最多1000个字符")]
    public string? ContentTemplate { get; set; }

    [StringLength(500, ErrorMessage = "描述最多500个字符")]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDefault { get; set; }
}

public class ReminderTemplateDto
{
    public int Id { get; set; }
    public ReminderTemplateType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TitleTemplate { get; set; } = string.Empty;
    public string ContentTemplate { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ReminderFeedbackDto
{
    [Required(ErrorMessage = "提醒记录ID不能为空")]
    public int ReminderRecordId { get; set; }

    public bool DisableFutureReminders { get; set; } = false;

    [StringLength(500, ErrorMessage = "反馈内容最多500个字符")]
    public string? Feedback { get; set; }
}
