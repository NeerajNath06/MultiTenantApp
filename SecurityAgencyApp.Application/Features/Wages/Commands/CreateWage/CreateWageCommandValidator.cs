using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Wages.Commands.CreateWage;

public class CreateWageCommandValidator : AbstractValidator<CreateWageCommand>
{
    public CreateWageCommandValidator()
    {
        RuleFor(x => x.WageSheetNumber)
            .NotEmpty().WithMessage("Wage sheet number is required")
            .MaximumLength(50).WithMessage("Wage sheet number must not exceed 50 characters");

        RuleFor(x => x.WagePeriodEnd)
            .GreaterThan(x => x.WagePeriodStart).WithMessage("Wage period end date must be after start date");

        RuleFor(x => x.WageDetails)
            .NotEmpty().WithMessage("At least one wage detail is required");

        RuleForEach(x => x.WageDetails).SetValidator(new WageDetailDtoValidator());
    }
}

public class WageDetailDtoValidator : AbstractValidator<WageDetailDto>
{
    public WageDetailDtoValidator()
    {
        RuleFor(x => x.GuardId)
            .NotEmpty().WithMessage("Guard ID is required");

        RuleFor(x => x.DaysWorked)
            .GreaterThanOrEqualTo(0).WithMessage("Days worked must be 0 or greater");

        RuleFor(x => x.HoursWorked)
            .GreaterThanOrEqualTo(0).WithMessage("Hours worked must be 0 or greater");

        RuleFor(x => x.BasicRate)
            .GreaterThanOrEqualTo(0).WithMessage("Basic rate must be 0 or greater");

        RuleFor(x => x.OvertimeHours)
            .GreaterThanOrEqualTo(0).WithMessage("Overtime hours must be 0 or greater");

        RuleFor(x => x.OvertimeRate)
            .GreaterThanOrEqualTo(0).WithMessage("Overtime rate must be 0 or greater");

        RuleFor(x => x.Allowances)
            .GreaterThanOrEqualTo(0).WithMessage("Allowances must be 0 or greater");

        RuleFor(x => x.Deductions)
            .GreaterThanOrEqualTo(0).WithMessage("Deductions must be 0 or greater");
    }
}
