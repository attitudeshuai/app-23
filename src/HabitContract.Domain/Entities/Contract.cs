using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class Contract : BaseEntity<int>
{
    public int OwnerId { get; set; }
    public int? TemplateId { get; set; }
    public string HabitName { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? PenaltyDescription { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Active;

    public User Owner { get; set; } = null!;
    public HabitTemplate? Template { get; set; }
    public ICollection<ContractPartner> Partners { get; set; } = new List<ContractPartner>();
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    public ICollection<ContractViolation> Violations { get; set; } = new List<ContractViolation>();
}
