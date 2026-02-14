using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Menus.Commands.CreateMenu;

public class CreateMenuCommandValidator : AbstractValidator<CreateMenuCommand>
{
    public CreateMenuCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Menu name is required")
            .MaximumLength(100).WithMessage("Menu name must not exceed 100 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater");
    }
}
