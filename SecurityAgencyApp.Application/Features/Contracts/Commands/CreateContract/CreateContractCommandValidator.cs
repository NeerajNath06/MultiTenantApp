using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Contracts.Commands.CreateContract;

public class CreateContractCommandValidator : AbstractValidator<CreateContractCommand>
{
    public CreateContractCommandValidator()
    {
        RuleFor(x => x.ContractNumber)
            .NotEmpty().WithMessage("Contract number is required")
            .MaximumLength(50).WithMessage("Contract number must not exceed 50 characters");

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.ContractValue)
            .GreaterThanOrEqualTo(0).WithMessage("Contract value must be 0 or greater");

        RuleFor(x => x.MonthlyAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Monthly amount must be 0 or greater");

        RuleFor(x => x.BillingCycle)
            .Must(cycle => new[] { "Monthly", "Quarterly", "Yearly", "OneTime" }.Contains(cycle))
            .WithMessage("Invalid billing cycle");
    }
}
