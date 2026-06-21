using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;

namespace HabitContract.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // 用户映射
        CreateMap<User, UserDto>();
        CreateMap<UserRegisterDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Avatar, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // 契约映射
        CreateMap<Contract, ContractDto>()
            .ForMember(dest => dest.OwnerName, opt => opt.Ignore())
            .ForMember(dest => dest.PartnerCount, opt => opt.Ignore())
            .ForMember(dest => dest.CheckInCount, opt => opt.Ignore())
            .ForMember(dest => dest.ViolationCount, opt => opt.Ignore());

        CreateMap<Contract, ContractListDto>()
            .ForMember(dest => dest.OwnerName, opt => opt.Ignore());

        CreateMap<ContractCreateDto, Contract>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Owner, opt => opt.Ignore())
            .ForMember(dest => dest.Partners, opt => opt.Ignore())
            .ForMember(dest => dest.CheckIns, opt => opt.Ignore())
            .ForMember(dest => dest.Violations, opt => opt.Ignore());

        // 监督伙伴映射
        CreateMap<ContractPartner, ContractPartnerDto>()
            .ForMember(dest => dest.ContractName, opt => opt.Ignore())
            .ForMember(dest => dest.PartnerName, opt => opt.Ignore());

        CreateMap<ContractPartnerCreateDto, ContractPartner>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.JoinedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Contract, opt => opt.Ignore())
            .ForMember(dest => dest.Partner, opt => opt.Ignore());

        // 打卡映射
        CreateMap<CheckIn, CheckInDto>()
            .ForMember(dest => dest.ContractName, opt => opt.Ignore())
            .ForMember(dest => dest.Username, opt => opt.Ignore());

        CreateMap<CheckIn, CheckInListDto>()
            .ForMember(dest => dest.ContractName, opt => opt.Ignore())
            .ForMember(dest => dest.Username, opt => opt.Ignore());

        CreateMap<CheckInCreateDto, CheckIn>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Contract, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // 违约映射
        CreateMap<ContractViolation, ContractViolationDto>()
            .ForMember(dest => dest.ContractName, opt => opt.Ignore());

        CreateMap<ContractViolationCreateDto, ContractViolation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Contract, opt => opt.Ignore());

        // 分页结果映射
        CreateMap(typeof(PagedResult<>), typeof(PagedResultDto<>));
    }
}
