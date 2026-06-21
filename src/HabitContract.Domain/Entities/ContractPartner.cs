using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Entities;

public class ContractPartner : BaseEntity<int>
{
    public int ContractId { get; set; }
    public int PartnerId { get; set; }
    public PartnerStatus Status { get; set; } = PartnerStatus.Pending;
    public DateTime? JoinedAt { get; set; }

    public Contract Contract { get; set; } = null!;
    public User Partner { get; set; } = null!;
}
