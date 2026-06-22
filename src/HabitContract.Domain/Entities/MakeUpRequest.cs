using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class MakeUpRequest : BaseEntity<int>
{
    public int ContractId { get; set; }
    public int UserId { get; set; }
    public DateTime CheckInDate { get; set; }
    public string? ProofText { get; set; }
    public string? ProofPhoto { get; set; }
    public string Reason { get; set; } = string.Empty;
    public MakeUpRequestStatus Status { get; set; } = MakeUpRequestStatus.Pending;
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }

    public Contract Contract { get; set; } = null!;
    public User User { get; set; } = null!;
    public User? Reviewer { get; set; }
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
}
