using FluentValidation;
using HabitContract.Application.DTOs;

namespace HabitContract.Application.Validators;

public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .MinimumLength(3).WithMessage("用户名至少3个字符")
            .MaximumLength(50).WithMessage("用户名最多50个字符");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .EmailAddress().WithMessage("邮箱格式不正确");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码至少6个字符")
            .MaximumLength(100).WithMessage("密码最多100个字符");
    }
}

public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("用户名或邮箱不能为空");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空");
    }
}

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(x => x.Username)
            .MinimumLength(3).WithMessage("用户名至少3个字符")
            .MaximumLength(50).WithMessage("用户名最多50个字符")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("邮箱格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("密码至少6个字符")
            .MaximumLength(100).WithMessage("密码最多100个字符")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}
