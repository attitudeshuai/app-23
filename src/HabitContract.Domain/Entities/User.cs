using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class User : BaseEntity<int>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Avatar { get; set; }

    public int CreditScore { get; set; } = 100;

    public decimal OutstandingPenaltyBalance { get; set; } = 0;

    public bool IsPaymentSuspended { get; set; } = false;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<ContractPartner> ContractPartners { get; set; } = new List<ContractPartner>();
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    public ICollection<PenaltyExecutionRecord> PenaltyExecutionRecords { get; set; } = new List<PenaltyExecutionRecord>();
}
