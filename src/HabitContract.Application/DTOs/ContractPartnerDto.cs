using System.ComponentModel.DataAnnotations;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.DTOs;

public class ContractPartnerCreateDto
{
    [Required(ErrorMessage = "契约ID不能为空")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "伙伴ID不能为空")]
    public int PartnerId { get; set; }

    public PartnerRole Role { get; set; } = PartnerRole.Supervisor;
}

public class ContractPartnerUpdateDto
{
    public PartnerRole? Role { get; set; }
    public PartnerStatus? Status { get; set; }
}

public class ContractPartnerStatusDto
{
    [Required(ErrorMessage = "状态不能为空")]
    public PartnerStatus Status { get; set; }
}

public class ContractPartnerDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public int PartnerId { get; set; }
    public string? PartnerName { get; set; }
    public PartnerRole Role { get; set; }
    public PartnerStatus Status { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
