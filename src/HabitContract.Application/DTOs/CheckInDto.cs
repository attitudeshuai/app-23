using System.ComponentModel.DataAnnotations;

namespace HabitContract.Application.DTOs;

public class CheckInCreateDto
{
    [Required(ErrorMessage = "契约ID不能为空")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "打卡日期不能为空")]
    public DateTime CheckInDate { get; set; }

    [StringLength(1000, ErrorMessage = "打卡证明文字最多1000个字符")]
    public string? ProofText { get; set; }

    [StringLength(500, ErrorMessage = "打卡证明图片链接最多500个字符")]
    public string? ProofPhoto { get; set; }
}

public class CheckInUpdateDto
{
    [StringLength(1000, ErrorMessage = "打卡证明文字最多1000个字符")]
    public string? ProofText { get; set; }

    [StringLength(500, ErrorMessage = "打卡证明图片链接最多500个字符")]
    public string? ProofPhoto { get; set; }
}

public class CheckInDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public DateTime CheckInDate { get; set; }
    public string? ProofText { get; set; }
    public string? ProofPhoto { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CheckInListDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
