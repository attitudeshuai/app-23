using HabitContract.Application.DTOs;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.Interfaces;

public interface IRoleChangeAuditService
{
    Task<RoleChangeAuditDto> CreateAuditRecordAsync(
        int contractId,
        int partnerId,
        PartnerRole oldRole,
        PartnerRole newRole,
        int changedByUserId,
        string changeReason);

    Task<List<RoleChangeAuditDto>> GetAuditsByContractIdAsync(int contractId);
    Task<List<RoleChangeAuditDto>> GetAuditsByPartnerIdAsync(int partnerId);
}
