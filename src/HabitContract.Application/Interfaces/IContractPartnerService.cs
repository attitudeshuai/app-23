using HabitContract.Application.DTOs;
using HabitContract.Domain.Common;

namespace HabitContract.Application.Interfaces;

public interface IContractPartnerService
{
    Task<PagedResultDto<ContractPartnerDto>> GetPartnersAsync(QueryParameters parameters);
    Task<ContractPartnerDto> GetPartnerByIdAsync(int id);
    Task<ContractPartnerDto> CreatePartnerAsync(int userId, ContractPartnerCreateDto dto);
    Task<ContractPartnerDto> UpdatePartnerAsync(int userId, int id, ContractPartnerUpdateDto dto);
    Task DeletePartnerAsync(int userId, int id);
    Task<ContractPartnerDto> UpdatePartnerStatusAsync(int userId, int id, ContractPartnerStatusDto dto);
}
