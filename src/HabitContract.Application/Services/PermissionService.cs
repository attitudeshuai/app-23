using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CanPerformOperationAsync(int userId, int contractId, ContractOperation operation)
    {
        var (isAllowed, _) = await CheckPermissionAsync(userId, contractId, operation);
        return isAllowed;
    }

    public async Task<(bool IsAllowed, string ErrorMessage)> CheckPermissionAsync(int userId, int contractId, ContractOperation operation)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
        if (contract == null)
        {
            return (false, "契约不存在");
        }

        if (contract.OwnerId == userId)
        {
            return (true, string.Empty);
        }

        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        var partner = allPartners.FirstOrDefault(p => p.ContractId == contractId && p.PartnerId == userId);

        if (partner == null || partner.Status != PartnerStatus.Accepted)
        {
            return (false, "无权限执行此操作");
        }

        var isAllowed = operation switch
        {
            ContractOperation.ViewContract => true,
            ContractOperation.ViewCheckIns => CanViewCheckIns(partner.Role),
            ContractOperation.CreateCheckIn => true,
            ContractOperation.EditCheckIn => true,
            ContractOperation.DeleteCheckIn => true,
            ContractOperation.InitiateMakeUpReview => CanInitiateMakeUpReview(partner.Role),
            ContractOperation.ApproveMakeUp => CanApproveMakeUp(partner.Role),
            ContractOperation.ReviewCheckIn => CanApproveMakeUp(partner.Role),
            ContractOperation.ViewViolations => CanViewViolations(partner.Role),
            ContractOperation.ViewStats => CanViewStats(partner.Role),
            ContractOperation.EditContract => false,
            ContractOperation.DeleteContract => false,
            ContractOperation.ChangeContractStatus => false,
            ContractOperation.ManagePartners => false,
            ContractOperation.ChangePartnerRole => false,
            _ => false
        };

        if (!isAllowed)
        {
            return (false, $"当前角色「{GetRoleDisplayName(partner.Role)}」无权限执行此操作");
        }

        return (true, string.Empty);
    }

    public async Task<PartnerRole?> GetUserRoleInContractAsync(int userId, int contractId)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
        if (contract == null)
        {
            return null;
        }

        if (contract.OwnerId == userId)
        {
            return null;
        }

        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        var partner = allPartners.FirstOrDefault(p => p.ContractId == contractId && p.PartnerId == userId);

        return partner?.Role;
    }

    public async Task<bool> IsContractOwnerAsync(int userId, int contractId)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
        return contract != null && contract.OwnerId == userId;
    }

    public async Task<bool> IsPartnerInContractAsync(int userId, int contractId)
    {
        var allPartners = await _unitOfWork.ContractPartners.GetAllAsync();
        return allPartners.Any(p => p.ContractId == contractId && p.PartnerId == userId && p.Status == PartnerStatus.Accepted);
    }

    private static bool CanViewCheckIns(PartnerRole role)
    {
        return role == PartnerRole.Supervisor ||
               role == PartnerRole.Witness ||
               role == PartnerRole.Supporter;
    }

    private static bool CanInitiateMakeUpReview(PartnerRole role)
    {
        return role == PartnerRole.Supervisor;
    }

    private static bool CanApproveMakeUp(PartnerRole role)
    {
        return role == PartnerRole.Supervisor;
    }

    private static bool CanViewViolations(PartnerRole role)
    {
        return role == PartnerRole.Supervisor ||
               role == PartnerRole.Witness;
    }

    private static bool CanViewStats(PartnerRole role)
    {
        return role == PartnerRole.Supervisor ||
               role == PartnerRole.Witness ||
               role == PartnerRole.Supporter;
    }

    private static string GetRoleDisplayName(PartnerRole role)
    {
        return role switch
        {
            PartnerRole.Supervisor => "监督人",
            PartnerRole.Witness => "见证人",
            PartnerRole.Supporter => "支持者",
            _ => role.ToString()
        };
    }
}
