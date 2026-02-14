using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentNumber)
            .NotEmpty().WithMessage("Payment number is required")
            .MaximumLength(50).WithMessage("Payment number must not exceed 50 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.PaymentMethod)
            .Must(method => new[] { "Cash", "Cheque", "BankTransfer", "UPI", "CreditCard", "DebitCard" }.Contains(method))
            .WithMessage("Invalid payment method");

        RuleFor(x => x.ChequeNumber)
            .NotEmpty().WithMessage("Cheque number is required")
            .When(x => x.PaymentMethod == "Cheque");

        RuleFor(x => x.TransactionReference)
            .NotEmpty().WithMessage("Transaction reference is required")
            .When(x => x.PaymentMethod == "BankTransfer" || x.PaymentMethod == "UPI");
    }
}
