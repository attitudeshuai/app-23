using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class ContractViolation : BaseEntity<int>
{
    public int ContractId { get; set; }
    public DateTime ViolationDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; } = false;

    public Contract Contract { get; set; } = null!;
}
