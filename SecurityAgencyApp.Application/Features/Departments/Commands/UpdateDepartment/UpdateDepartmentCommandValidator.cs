using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Department ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required")
            .MaximumLength(100).WithMessage("Department name must not exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Department code is required")
            .MaximumLength(20).WithMessage("Department code must not exceed 20 characters");
    }
}
