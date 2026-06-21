using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Application.Services;

public class CheckInService : ICheckInService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CheckInService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<CheckInListDto>> GetCheckInsAsync(QueryParameters parameters)
    {
        var pagedResult = await _unitOfWork.CheckIns.GetPagedAsync(parameters);
        return await MapToPagedCheckInListDto(pagedResult);
    }

    public async Task<CheckInDto> GetCheckInByIdAsync(int id)
    {
        var checkIn = await _unitOfWork.CheckIns.GetByIdAsync(id);
        if (checkIn == null)
        {
            throw new BusinessException("打卡记录不存在", 404);
        }

        return await MapToCheckInDto(checkIn);
    }

    public async Task<CheckInDto> CreateCheckInAsync(int userId, CheckInCreateDto dto)
    {
        // 验证契约是否存在且为活跃状态
        var contract = await _unitOfWork.Contracts.GetByIdAsync(dto.ContractId);
        if (contract == null)
        {
            throw new BusinessException("契约不存在");
        }

        if (contract.Status != ContractStatus.Active)
        {
            throw new BusinessException("只能为进行中的契约打卡");
        }

        // 检查今天是否已打卡
        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);
        var alreadyCheckedIn = allCheckIns.Any(ci =>
            ci.ContractId == dto.ContractId &&
            ci.UserId == userId &&
            ci.CheckInDate >= todayStart &&
            ci.CheckInDate < todayEnd);

        if (alreadyCheckedIn)
        {
            throw new BusinessException("今天已经打过卡了");
        }

        var checkIn = _mapper.Map<CheckIn>(dto);
        checkIn.UserId = userId;

        var created = await _unitOfWork.CheckIns.AddAsync(checkIn);
        await _unitOfWork.SaveChangesAsync();

        return await MapToCheckInDto(created);
    }

    public async Task<CheckInDto> UpdateCheckInAsync(int userId, int id, CheckInUpdateDto dto)
    {
        var checkIn = await _unitOfWork.CheckIns.GetByIdAsync(id);
        if (checkIn == null)
        {
            throw new BusinessException("打卡记录不存在", 404);
        }

        // 只有打卡创建者可以修改
        if (checkIn.UserId != userId)
        {
            throw new BusinessException("无权限修改此打卡记录", 403);
        }

        if (dto.ProofText != null)
            checkIn.ProofText = dto.ProofText;

        if (dto.ProofPhoto != null)
            checkIn.ProofPhoto = dto.ProofPhoto;

        checkIn.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.CheckIns.UpdateAsync(checkIn);
        await _unitOfWork.SaveChangesAsync();

        return await MapToCheckInDto(checkIn);
    }

    public async Task DeleteCheckInAsync(int userId, int id)
    {
        var checkIn = await _unitOfWork.CheckIns.GetByIdAsync(id);
        if (checkIn == null)
        {
            throw new BusinessException("打卡记录不存在", 404);
        }

        // 只有打卡创建者可以删除
        if (checkIn.UserId != userId)
        {
            throw new BusinessException("无权限删除此打卡记录", 403);
        }

        await _unitOfWork.CheckIns.DeleteAsync(checkIn);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResultDto<CheckInListDto>> GetMyCheckInsAsync(int userId, QueryParameters parameters)
    {
        // 获取用户的所有打卡记录并手动分页
        var allCheckIns = await _unitOfWork.CheckIns.GetAllAsync();
        var myCheckIns = allCheckIns.Where(ci => ci.UserId == userId).ToList();

        var totalCount = myCheckIns.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
        var pagedItems = myCheckIns
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var items = new List<CheckInListDto>();
        foreach (var checkIn in pagedItems)
        {
            var dto = await MapToCheckInListDto(checkIn);
            items.Add(dto);
        }

        return new PagedResultDto<CheckInListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = parameters.PageNumber > 1,
            HasNextPage = parameters.PageNumber < totalPages
        };
    }

    /// <summary>
    /// 将CheckIn映射为CheckInDto（包含关联名称）
    /// </summary>
    private async Task<CheckInDto> MapToCheckInDto(CheckIn checkIn)
    {
        var dto = _mapper.Map<CheckInDto>(checkIn);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(checkIn.ContractId);
        dto.ContractName = contract?.HabitName;

        var user = await _unitOfWork.Users.GetByIdAsync(checkIn.UserId);
        dto.Username = user?.Username;

        return dto;
    }

    /// <summary>
    /// 将CheckIn映射为CheckInListDto（包含关联名称）
    /// </summary>
    private async Task<CheckInListDto> MapToCheckInListDto(CheckIn checkIn)
    {
        var dto = _mapper.Map<CheckInListDto>(checkIn);

        var contract = await _unitOfWork.Contracts.GetByIdAsync(checkIn.ContractId);
        dto.ContractName = contract?.HabitName;

        var user = await _unitOfWork.Users.GetByIdAsync(checkIn.UserId);
        dto.Username = user?.Username;

        return dto;
    }

    /// <summary>
    /// 将分页结果映射为CheckInListDto分页
    /// </summary>
    private async Task<PagedResultDto<CheckInListDto>> MapToPagedCheckInListDto(PagedResult<CheckIn> pagedResult)
    {
        var items = new List<CheckInListDto>();
        foreach (var checkIn in pagedResult.Items)
        {
            var dto = await MapToCheckInListDto(checkIn);
            items.Add(dto);
        }

        return new PagedResultDto<CheckInListDto>
        {
            Items = items,
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalPages = pagedResult.TotalPages,
            HasPreviousPage = pagedResult.PageNumber > 1,
            HasNextPage = pagedResult.PageNumber < pagedResult.TotalPages
        };
    }
}
