using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class RoleChangeAuditService : IRoleChangeAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoleChangeAuditService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RoleChangeAuditDto> CreateAuditRecordAsync(
        int contractId,
        int partnerId,
        PartnerRole oldRole,
        PartnerRole newRole,
        int changedByUserId,
        string changeReason)
    {
        var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
        var partnerUser = await _unitOfWork.Users.GetByIdAsync(partnerId);
        var changedByUser = await _unitOfWork.Users.GetByIdAsync(changedByUserId);

        var audit = new RoleChangeAudit
        {
            ContractId = contractId,
            PartnerId = partnerId,
            OldRole = oldRole,
            NewRole = newRole,
            ChangedByUserId = changedByUserId,
            ChangeReason = changeReason,
            ContractName = contract?.HabitName,
            PartnerUsername = partnerUser?.Username,
            ChangedByUsername = changedByUser?.Username,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.RoleChangeAudits.AddAsync(audit);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<RoleChangeAuditDto>(created);
    }

    public async Task<List<RoleChangeAuditDto>> GetAuditsByContractIdAsync(int contractId)
    {
        var audits = await _unitOfWork.RoleChangeAudits.GetByContractIdAsync(contractId);
        return _mapper.Map<List<RoleChangeAuditDto>>(audits);
    }

    public async Task<List<RoleChangeAuditDto>> GetAuditsByPartnerIdAsync(int partnerId)
    {
        var audits = await _unitOfWork.RoleChangeAudits.GetByPartnerIdAsync(partnerId);
        return _mapper.Map<List<RoleChangeAuditDto>>(audits);
    }
}
