using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Bills.Commands.UpdateBill;

public class UpdateBillCommandValidator : AbstractValidator<UpdateBillCommand>
{
    public UpdateBillCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Bill ID is required");

        RuleFor(x => x.BillNumber)
            .NotEmpty().WithMessage("Bill number is required")
            .MaximumLength(50).WithMessage("Bill number must not exceed 50 characters");

        RuleFor(x => x.BillDate)
            .NotEmpty().WithMessage("Bill date is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Items)
            .SetValidator(new BillItemDtoValidator());
    }
}

public class BillItemDtoValidator : AbstractValidator<BillItemDto>
{
    public BillItemDtoValidator()
    {
        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage("Item name is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to zero");

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("Tax rate must be greater than or equal to zero")
            .LessThanOrEqualTo(100).WithMessage("Tax rate must not exceed 100");
    }
}
