using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Contracts.Commands.UpdateContract;

public class UpdateContractCommandValidator : AbstractValidator<UpdateContractCommand>
{
    public UpdateContractCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Contract ID is required");

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
    }
}
