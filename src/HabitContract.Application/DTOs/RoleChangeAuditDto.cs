using HabitContract.Domain.Enums;

namespace HabitContract.Application.DTOs;

public class RoleChangeAuditDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ContractName { get; set; }
    public int PartnerId { get; set; }
    public string? PartnerUsername { get; set; }
    public PartnerRole OldRole { get; set; }
    public PartnerRole NewRole { get; set; }
    public int ChangedByUserId { get; set; }
    public string? ChangedByUsername { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
