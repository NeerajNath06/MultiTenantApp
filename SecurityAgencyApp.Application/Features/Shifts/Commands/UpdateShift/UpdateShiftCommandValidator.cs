using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Shifts.Commands.UpdateShift;

public class UpdateShiftCommandValidator : AbstractValidator<UpdateShiftCommand>
{
    public UpdateShiftCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Shift ID is required");

        RuleFor(x => x.ShiftName)
            .NotEmpty().WithMessage("Shift name is required")
            .MaximumLength(100).WithMessage("Shift name must not exceed 100 characters");

        RuleFor(x => x.BreakDuration)
            .GreaterThanOrEqualTo(0).WithMessage("Break duration must be 0 or greater");
    }
}
