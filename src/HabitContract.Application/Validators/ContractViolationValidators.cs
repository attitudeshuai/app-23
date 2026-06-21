using FluentValidation;
using HabitContract.Application.DTOs;

namespace HabitContract.Application.Validators;

public class ContractViolationCreateDtoValidator : AbstractValidator<ContractViolationCreateDto>
{
    public ContractViolationCreateDtoValidator()
    {
        RuleFor(x => x.ContractId)
            .GreaterThan(0).WithMessage("契约ID必须大于0");

        RuleFor(x => x.ViolationDate)
            .NotEmpty().WithMessage("违约日期不能为空");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("违约原因不能为空")
            .MaximumLength(500).WithMessage("违约原因最多500个字符");
    }
}

public class ContractViolationUpdateDtoValidator : AbstractValidator<ContractViolationUpdateDto>
{
    public ContractViolationUpdateDtoValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("违约原因最多500个字符")
            .When(x => x.Reason != null);
    }
}
