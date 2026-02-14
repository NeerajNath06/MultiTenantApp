using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Designations.Commands.CreateDesignation;

public class CreateDesignationCommandValidator : AbstractValidator<CreateDesignationCommand>
{
    public CreateDesignationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Designation name is required")
            .MaximumLength(100).WithMessage("Designation name must not exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Designation code is required")
            .MaximumLength(20).WithMessage("Designation code must not exceed 20 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}
