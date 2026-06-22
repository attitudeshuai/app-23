using HabitContract.Application.DTOs;
using HabitContract.Domain.Common;

namespace HabitContract.Application.Interfaces;

public interface ICheckInService
{
    Task<PagedResultDto<CheckInListDto>> GetCheckInsAsync(QueryParameters parameters);
    Task<CheckInDto> GetCheckInByIdAsync(int userId, int id);
    Task<CheckInDto> CreateCheckInAsync(int userId, CheckInCreateDto dto);
    Task<CheckInDto> UpdateCheckInAsync(int userId, int id, CheckInUpdateDto dto);
    Task DeleteCheckInAsync(int userId, int id);
    Task<PagedResultDto<CheckInListDto>> GetMyCheckInsAsync(int userId, QueryParameters parameters);
    Task ReValidateRecentCheckInsAsync(int contractId, FrequencyRule newRule);
}
