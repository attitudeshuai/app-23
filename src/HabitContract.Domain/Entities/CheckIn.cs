using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class CheckIn : BaseEntity<int>
{
    public int ContractId { get; set; }
    public int UserId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckInTime { get; set; }
    public string? ProofText { get; set; }
    public string? ProofPhoto { get; set; }
    public CheckInStatus Status { get; set; } = CheckInStatus.Pending;
    public DateTime? StatusChangedAt { get; set; }
    public int? MakeUpRequestId { get; set; }
    public int ConsecutiveDays { get; set; }

    public Contract Contract { get; set; } = null!;
    public User User { get; set; } = null!;
    public MakeUpRequest? MakeUpRequest { get; set; }
}
