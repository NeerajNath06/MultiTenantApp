using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Menus.Commands.UpdateMenu;

public class UpdateMenuCommandValidator : AbstractValidator<UpdateMenuCommand>
{
    public UpdateMenuCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Menu ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Menu name is required")
            .MaximumLength(100).WithMessage("Menu name must not exceed 100 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater");
    }
}
