using FluentValidation;
using HabitContract.Application.DTOs;

namespace HabitContract.Application.Validators;

public class CheckInCreateDtoValidator : AbstractValidator<CheckInCreateDto>
{
    public CheckInCreateDtoValidator()
    {
        RuleFor(x => x.ContractId)
            .GreaterThan(0).WithMessage("契约ID必须大于0");

        RuleFor(x => x.CheckInDate)
            .NotEmpty().WithMessage("打卡日期不能为空");

        RuleFor(x => x.ProofText)
            .MaximumLength(1000).WithMessage("打卡证明文字最多1000个字符")
            .When(x => x.ProofText != null);

        RuleFor(x => x.ProofPhoto)
            .MaximumLength(500).WithMessage("打卡证明图片链接最多500个字符")
            .When(x => x.ProofPhoto != null);
    }
}

public class CheckInUpdateDtoValidator : AbstractValidator<CheckInUpdateDto>
{
    public CheckInUpdateDtoValidator()
    {
        RuleFor(x => x.ProofText)
            .MaximumLength(1000).WithMessage("打卡证明文字最多1000个字符")
            .When(x => x.ProofText != null);

        RuleFor(x => x.ProofPhoto)
            .MaximumLength(500).WithMessage("打卡证明图片链接最多500个字符")
            .When(x => x.ProofPhoto != null);
    }
}
