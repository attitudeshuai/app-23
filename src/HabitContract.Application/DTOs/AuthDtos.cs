using System.ComponentModel.DataAnnotations;

namespace HabitContract.Application.DTOs;

public class UserRegisterDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度应在3-50个字符之间")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度应在6-100个字符之间")]
    public string Password { get; set; } = string.Empty;
}

public class UserLoginDto
{
    [Required(ErrorMessage = "用户名或邮箱不能为空")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserUpdateDto
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度应在3-50个字符之间")]
    public string? Username { get; set; }

    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度应在6-100个字符之间")]
    public string? Password { get; set; }

    public string? Avatar { get; set; }
}
