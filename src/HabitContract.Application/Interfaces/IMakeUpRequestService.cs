using HabitContract.Application.DTOs;
using HabitContract.Domain.Common;

namespace HabitContract.Application.Interfaces;

public interface IMakeUpRequestService
{
    Task<PagedResultDto<MakeUpRequestListDto>> GetMakeUpRequestsAsync(QueryParameters parameters);
    Task<MakeUpRequestDto> GetMakeUpRequestByIdAsync(int userId, int id);
    Task<MakeUpRequestDto> CreateMakeUpRequestAsync(int userId, MakeUpRequestCreateDto dto);
    Task<MakeUpRequestDto> ReviewMakeUpRequestAsync(int reviewerId, int id, MakeUpRequestReviewDto dto);
    Task<PagedResultDto<MakeUpRequestListDto>> GetMyMakeUpRequestsAsync(int userId, QueryParameters parameters);
    Task<PagedResultDto<MakeUpRequestListDto>> GetPendingReviewsAsync(int reviewerId, QueryParameters parameters);
}
