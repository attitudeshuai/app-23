using AutoMapper;
using HabitContract.Application.DTOs;
using HabitContract.Application.Interfaces;
using HabitContract.Domain.Common;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace HabitContract.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _mapper = mapper;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<LoginResponseDto> RegisterAsync(UserRegisterDto dto)
    {
        // 检查用户名是否已存在
        var allUsers = await _unitOfWork.Users.GetAllAsync();
        if (allUsers.Any(u => u.Username == dto.Username))
        {
            throw new BusinessException("用户名已被使用");
        }

        // 检查邮箱是否已注册
        if (allUsers.Any(u => u.Email == dto.Email))
        {
            throw new BusinessException("邮箱已被注册");
        }

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        var createdUser = await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return BuildLoginResponse(createdUser);
    }

    public async Task<LoginResponseDto> LoginAsync(UserLoginDto dto)
    {
        // 根据用户名或邮箱查找用户
        var allUsers = await _unitOfWork.Users.GetAllAsync();
        var user = allUsers.FirstOrDefault(u => u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);

        if (user == null)
        {
            throw new BusinessException("用户名/邮箱或密码错误");
        }

        // 验证密码
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new BusinessException("用户名/邮箱或密码错误");
        }

        return BuildLoginResponse(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new BusinessException("用户不存在", 404);
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateCurrentUserAsync(int userId, UserUpdateDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new BusinessException("用户不存在", 404);
        }

        var allUsers = await _unitOfWork.Users.GetAllAsync();

        // 修改用户名时检查唯一性
        if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
        {
            if (allUsers.Any(u => u.Username == dto.Username && u.Id != userId))
            {
                throw new BusinessException("用户名已被使用");
            }
            user.Username = dto.Username;
        }

        // 修改邮箱时检查唯一性
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            if (allUsers.Any(u => u.Email == dto.Email && u.Id != userId))
            {
                throw new BusinessException("邮箱已被注册");
            }
            user.Email = dto.Email;
        }

        // 修改密码
        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
        }

        if (dto.Avatar != null)
        {
            user.Avatar = dto.Avatar;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    /// <summary>
    /// 构建登录响应（生成JWT令牌和用户信息）
    /// </summary>
    private LoginResponseDto BuildLoginResponse(User user)
    {
        var token = _jwtService.GenerateToken(user.Id, user.Username, user.Email);
        var userDto = _mapper.Map<UserDto>(user);
        var expiryMinutes = 1440;

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            User = userDto
        };
    }
}
