using System.ComponentModel.DataAnnotations;

namespace HabitContract.Application.DTOs;

public class ContractViolationCreateDto
{
    [Required(ErrorMessage = "契约ID不能为空")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "违约日期不能为空")]
    public DateTime ViolationDate { get; set; }

    [Required(ErrorMessage = "违约原因不能为空")]
    [StringLength(500, ErrorMessage = "违约原因最多500个字符")]
    public string Reason { get; set; } = string.Empty;
}

public class ContractViolationUpdateDto
{
    [StringLength(500, ErrorMessage = "违约原因最多500个字符")]
    public string? Reason { get; set; }

    public bool? IsConfirmed { get; set; }
}

public class ContractViolationDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public DateTime ViolationDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}
