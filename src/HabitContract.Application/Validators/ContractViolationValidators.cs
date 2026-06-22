using FluentValidation;
using HabitContract.Application.DTOs;
using HabitContract.Domain.Enums;

namespace HabitContract.Application.Validators;

public class ContractViolationCreateDtoValidator : AbstractValidator<ContractViolationCreateDto>
{
    public ContractViolationCreateDtoValidator()
    {
        RuleFor(x => x.ContractId)
            .GreaterThan(0).WithMessage("契约ID必须大于0");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("违约用户ID必须大于0");

        RuleFor(x => x.ViolationDate)
            .NotEmpty().WithMessage("违约日期不能为空");

        RuleFor(x => x.ViolationType)
            .IsInEnum().WithMessage("无效的违约类型");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("违约原因不能为空")
            .MaximumLength(500).WithMessage("违约原因最多500个字符");
    }
}

public class ContractViolationUpdateDtoValidator : AbstractValidator<ContractViolationUpdateDto>
{
    public ContractViolationUpdateDtoValidator()
    {
        RuleFor(x => x.ViolationType)
            .IsInEnum().WithMessage("无效的违约类型")
            .When(x => x.ViolationType.HasValue);

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("违约原因最多500个字符")
            .When(x => x.Reason != null);
    }
}
