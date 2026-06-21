using FluentValidation;
using HabitContract.Application.DTOs;

namespace HabitContract.Application.Validators;

public class ContractCreateDtoValidator : AbstractValidator<ContractCreateDto>
{
    public ContractCreateDtoValidator()
    {
        RuleFor(x => x.HabitName)
            .NotEmpty().WithMessage("习惯名称不能为空")
            .MaximumLength(100).WithMessage("习惯名称最多100个字符");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("频率不能为空")
            .MaximumLength(50).WithMessage("频率最多50个字符");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("开始日期不能为空");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("结束日期不能为空")
            .GreaterThan(x => x.StartDate).WithMessage("结束日期必须大于开始日期");

        RuleFor(x => x.PenaltyDescription)
            .MaximumLength(500).WithMessage("惩罚描述最多500个字符");
    }
}

public class ContractUpdateDtoValidator : AbstractValidator<ContractUpdateDto>
{
    public ContractUpdateDtoValidator()
    {
        RuleFor(x => x.HabitName)
            .MaximumLength(100).WithMessage("习惯名称最多100个字符")
            .When(x => !string.IsNullOrEmpty(x.HabitName));

        RuleFor(x => x.Frequency)
            .MaximumLength(50).WithMessage("频率最多50个字符")
            .When(x => !string.IsNullOrEmpty(x.Frequency));

        RuleFor(x => x.PenaltyDescription)
            .MaximumLength(500).WithMessage("惩罚描述最多500个字符")
            .When(x => x.PenaltyDescription != null);
    }
}
