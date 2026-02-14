using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Bills.Commands.CreateBill;

public class CreateBillCommandValidator : AbstractValidator<CreateBillCommand>
{
    public CreateBillCommandValidator()
    {
        RuleFor(x => x.BillNumber)
            .NotEmpty().WithMessage("Bill number is required")
            .MaximumLength(50).WithMessage("Bill number must not exceed 50 characters");

        RuleFor(x => x.ClientName)
            .NotEmpty().WithMessage("Client name is required")
            .MaximumLength(200).WithMessage("Client name must not exceed 200 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one bill item is required");

        RuleForEach(x => x.Items).SetValidator(new BillItemDtoValidator());
    }
}

public class BillItemDtoValidator : AbstractValidator<BillItemDto>
{
    public BillItemDtoValidator()
    {
        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage("Item name is required")
            .MaximumLength(200).WithMessage("Item name must not exceed 200 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be 0 or greater");

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("Tax rate must be 0 or greater")
            .LessThanOrEqualTo(100).WithMessage("Tax rate must not exceed 100");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount must be 0 or greater");
    }
}
