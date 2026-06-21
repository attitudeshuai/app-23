using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class CheckIn : BaseEntity<int>
{
    public int ContractId { get; set; }
    public int UserId { get; set; }
    public DateTime CheckInDate { get; set; }
    public string? ProofText { get; set; }
    public string? ProofPhoto { get; set; }

    public Contract Contract { get; set; } = null!;
    public User User { get; set; } = null!;
}
