using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Payments.Commands.UpdatePayment;

public class UpdatePaymentCommandValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Payment ID is required");

        RuleFor(x => x.PaymentNumber)
            .NotEmpty().WithMessage("Payment number is required")
            .MaximumLength(50).WithMessage("Payment number must not exceed 50 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required");
    }
}
