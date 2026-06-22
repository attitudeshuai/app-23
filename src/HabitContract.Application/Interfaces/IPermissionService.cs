using HabitContract.Domain.Enums;

namespace HabitContract.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> CanPerformOperationAsync(int userId, int contractId, ContractOperation operation);

    Task<(bool IsAllowed, string ErrorMessage)> CheckPermissionAsync(int userId, int contractId, ContractOperation operation);

    Task<PartnerRole?> GetUserRoleInContractAsync(int userId, int contractId);

    Task<bool> IsContractOwnerAsync(int userId, int contractId);

    Task<bool> IsPartnerInContractAsync(int userId, int contractId);
}
