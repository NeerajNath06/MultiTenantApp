using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Expenses.Commands.CreateExpense;

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.ExpenseNumber)
            .NotEmpty().WithMessage("Expense number is required")
            .MaximumLength(50).WithMessage("Expense number must not exceed 50 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(50).WithMessage("Category must not exceed 50 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.PaymentMethod)
            .Must(method => new[] { "Cash", "Cheque", "BankTransfer", "UPI", "CreditCard" }.Contains(method))
            .WithMessage("Invalid payment method");
    }
}
