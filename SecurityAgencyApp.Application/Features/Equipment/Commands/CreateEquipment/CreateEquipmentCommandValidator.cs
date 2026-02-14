using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Equipment.Commands.CreateEquipment;

public class CreateEquipmentCommandValidator : AbstractValidator<CreateEquipmentCommand>
{
    public CreateEquipmentCommandValidator()
    {
        RuleFor(x => x.EquipmentCode)
            .NotEmpty().WithMessage("Equipment code is required")
            .MaximumLength(50).WithMessage("Equipment code must not exceed 50 characters");

        RuleFor(x => x.EquipmentName)
            .NotEmpty().WithMessage("Equipment name is required")
            .MaximumLength(200).WithMessage("Equipment name must not exceed 200 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(50).WithMessage("Category must not exceed 50 characters");

        RuleFor(x => x.PurchaseCost)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase cost must be 0 or greater");

        RuleFor(x => x.Status)
            .Must(status => new[] { "Available", "Assigned", "Maintenance", "Damaged", "Retired" }.Contains(status))
            .WithMessage("Invalid status");
    }
}
