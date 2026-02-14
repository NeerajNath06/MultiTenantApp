using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.ClientCode)
            .NotEmpty().WithMessage("Client code is required")
            .MaximumLength(50).WithMessage("Client code must not exceed 50 characters");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.GSTNumber)
            .MaximumLength(50).WithMessage("GST number must not exceed 50 characters");

        RuleFor(x => x.PANNumber)
            .MaximumLength(50).WithMessage("PAN number must not exceed 50 characters");
    }
}
