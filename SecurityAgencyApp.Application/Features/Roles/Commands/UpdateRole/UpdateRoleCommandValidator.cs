using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Role ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(100).WithMessage("Role name must not exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Role code is required")
            .MaximumLength(20).WithMessage("Role code must not exceed 20 characters");
    }
}
