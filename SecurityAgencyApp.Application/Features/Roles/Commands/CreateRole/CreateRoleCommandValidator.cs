using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(100).WithMessage("Role name must not exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Role code is required")
            .MaximumLength(20).WithMessage("Role code must not exceed 20 characters");
    }
}
