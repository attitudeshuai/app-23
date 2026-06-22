using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class RoleChangeAudit : BaseEntity<int>
{
    public int ContractId { get; set; }
    public int PartnerId { get; set; }
    public PartnerRole OldRole { get; set; }
    public PartnerRole NewRole { get; set; }
    public int ChangedByUserId { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public string? ChangedByUsername { get; set; }
    public string? PartnerUsername { get; set; }
    public string? ContractName { get; set; }
}
