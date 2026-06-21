using HabitContract.Application.DTOs;
using HabitContract.Domain.Common;

namespace HabitContract.Application.Interfaces;

public interface IContractViolationService
{
    Task<PagedResultDto<ContractViolationDto>> GetViolationsAsync(QueryParameters parameters);
    Task<ContractViolationDto> GetViolationByIdAsync(int id);
    Task<ContractViolationDto> CreateViolationAsync(int userId, ContractViolationCreateDto dto);
    Task<ContractViolationDto> UpdateViolationAsync(int userId, int id, ContractViolationUpdateDto dto);
    Task DeleteViolationAsync(int userId, int id);
}
