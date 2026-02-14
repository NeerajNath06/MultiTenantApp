using FluentValidation;

namespace SecurityAgencyApp.Application.Features.SecurityGuards.Commands.UpdateGuard;

public class UpdateGuardCommandValidator : AbstractValidator<UpdateGuardCommand>
{
    public UpdateGuardCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Guard ID is required");

        RuleFor(x => x.GuardCode)
            .NotEmpty().WithMessage("Guard code is required")
            .MaximumLength(50).WithMessage("Guard code must not exceed 50 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters");
    }
}
