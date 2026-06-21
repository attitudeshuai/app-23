using System.ComponentModel.DataAnnotations;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.DTOs;

public class TemplateCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public int TemplateCount { get; set; }
}

public class TemplateCategoryCreateDto
{
    [Required(ErrorMessage = "分类名称不能为空")]
    [StringLength(50, ErrorMessage = "分类名称最多50个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "描述最多200个字符")]
    public string? Description { get; set; }

    [StringLength(500, ErrorMessage = "图标地址最多500个字符")]
    public string? Icon { get; set; }

    public int SortOrder { get; set; }
}

public class TemplateCategoryUpdateDto
{
    [StringLength(50, ErrorMessage = "分类名称最多50个字符")]
    public string? Name { get; set; }

    [StringLength(200, ErrorMessage = "描述最多200个字符")]
    public string? Description { get; set; }

    [StringLength(500, ErrorMessage = "图标地址最多500个字符")]
    public string? Icon { get; set; }

    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}

public class TemplateDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string DefaultFrequency { get; set; } = string.Empty;
    public int DefaultDurationDays { get; set; }
    public string DefaultGoalDescription { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int UsageCount { get; set; }
    public double CompletionRate { get; set; }
}

public class TemplateDetailDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string DefaultFrequency { get; set; } = string.Empty;
    public int DefaultDurationDays { get; set; }
    public string DefaultGoalDescription { get; set; } = string.Empty;
    public string DefaultSupervisorRule { get; set; } = string.Empty;
    public string? DefaultPenaltyDescription { get; set; }
    public string Version { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public double CompletionRate { get; set; }
}

public class TemplateDraftDto
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string HabitName { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string GoalDescription { get; set; } = string.Empty;
    public string SupervisorRule { get; set; } = string.Empty;
    public string? PenaltyDescription { get; set; }
}

public class TemplateCreateDto
{
    [Required(ErrorMessage = "分类ID不能为空")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "模板名称不能为空")]
    [StringLength(100, ErrorMessage = "模板名称最多100个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "描述最多500个字符")]
    public string? Description { get; set; }

    [StringLength(500, ErrorMessage = "图标地址最多500个字符")]
    public string? Icon { get; set; }

    [Required(ErrorMessage = "默认频率不能为空")]
    [StringLength(50, ErrorMessage = "默认频率最多50个字符")]
    public string DefaultFrequency { get; set; } = string.Empty;

    [Range(1, 3650, ErrorMessage = "持续天数必须在1-3650天之间")]
    public int DefaultDurationDays { get; set; } = 30;

    [Required(ErrorMessage = "默认目标描述不能为空")]
    [StringLength(500, ErrorMessage = "默认目标描述最多500个字符")]
    public string DefaultGoalDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "默认监督规则不能为空")]
    [StringLength(500, ErrorMessage = "默认监督规则最多500个字符")]
    public string DefaultSupervisorRule { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "默认惩罚描述最多500个字符")]
    public string? DefaultPenaltyDescription { get; set; }

    public int SortOrder { get; set; }
}

public class TemplateUpdateDto
{
    public int? CategoryId { get; set; }

    [StringLength(100, ErrorMessage = "模板名称最多100个字符")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "描述最多500个字符")]
    public string? Description { get; set; }

    [StringLength(500, ErrorMessage = "图标地址最多500个字符")]
    public string? Icon { get; set; }

    [StringLength(50, ErrorMessage = "默认频率最多50个字符")]
    public string? DefaultFrequency { get; set; }

    [Range(1, 3650, ErrorMessage = "持续天数必须在1-3650天之间")]
    public int? DefaultDurationDays { get; set; }

    [StringLength(500, ErrorMessage = "默认目标描述最多500个字符")]
    public string? DefaultGoalDescription { get; set; }

    [StringLength(500, ErrorMessage = "默认监督规则最多500个字符")]
    public string? DefaultSupervisorRule { get; set; }

    [StringLength(500, ErrorMessage = "默认惩罚描述最多500个字符")]
    public string? DefaultPenaltyDescription { get; set; }

    public int? SortOrder { get; set; }

    [StringLength(500, ErrorMessage = "变更说明最多500个字符")]
    public string? ChangeLog { get; set; }
}

public class TemplateStatusDto
{
    [Required(ErrorMessage = "状态不能为空")]
    public TemplateStatus Status { get; set; }
}

public class TemplateVersionDto
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DefaultFrequency { get; set; } = string.Empty;
    public int DefaultDurationDays { get; set; }
    public string DefaultGoalDescription { get; set; } = string.Empty;
    public string DefaultSupervisorRule { get; set; } = string.Empty;
    public string? DefaultPenaltyDescription { get; set; }
    public string? ChangeLog { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateContractFromTemplateDto
{
    [Required(ErrorMessage = "模板ID不能为空")]
    public int TemplateId { get; set; }

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
}
