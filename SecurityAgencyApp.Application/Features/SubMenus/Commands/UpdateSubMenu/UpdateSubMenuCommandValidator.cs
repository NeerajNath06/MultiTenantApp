using FluentValidation;

namespace SecurityAgencyApp.Application.Features.SubMenus.Commands.UpdateSubMenu;

public class UpdateSubMenuCommandValidator : AbstractValidator<UpdateSubMenuCommand>
{
    public UpdateSubMenuCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("SubMenu ID is required");

        RuleFor(x => x.MenuId)
            .NotEmpty().WithMessage("Menu ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("SubMenu name is required")
            .MaximumLength(100).WithMessage("SubMenu name must not exceed 100 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater");
    }
}
