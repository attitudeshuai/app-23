using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class ContractViolation : BaseEntity<int>
{
    public int ContractId { get; set; }
    public int UserId { get; set; }
    public DateTime ViolationDate { get; set; }
    public ViolationType ViolationType { get; set; } = ViolationType.Other;
    public bool IsSevere { get; set; } = false;
    public string Reason { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; } = false;

    public Contract Contract { get; set; } = null!;
    public User User { get; set; } = null!;
}
