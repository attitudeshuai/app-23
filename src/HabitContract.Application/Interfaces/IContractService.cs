using HabitContract.Application.DTOs;
using HabitContract.Domain.Common;

namespace HabitContract.Application.Interfaces;

public interface IContractService
{
    Task<PagedResultDto<ContractListDto>> GetContractsAsync(QueryParameters parameters);
    Task<ContractDto> GetContractByIdAsync(int id);
    Task<ContractDto> CreateContractAsync(int userId, ContractCreateDto dto);
    Task<ContractDto> UpdateContractAsync(int userId, int id, ContractUpdateDto dto);
    Task DeleteContractAsync(int userId, int id);
    Task<ContractDto> UpdateContractStatusAsync(int userId, int id, ContractStatusDto dto);
    Task<PagedResultDto<ContractListDto>> GetMyContractsAsync(int userId, QueryParameters parameters);
}
