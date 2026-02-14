using FluentValidation;

namespace SecurityAgencyApp.Application.Features.Authentication.Commands.RegisterAgency;

public class RegisterAgencyCommandValidator : AbstractValidator<RegisterAgencyCommand>
{
    public RegisterAgencyCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters");

        RuleFor(x => x.RegistrationNumber)
            .NotEmpty().WithMessage("Registration number is required")
            .MaximumLength(50).WithMessage("Registration number must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.AdminUserName)
            .NotEmpty().WithMessage("Admin username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("Admin email is required")
            .EmailAddress().WithMessage("Invalid admin email format")
            .MaximumLength(100).WithMessage("Admin email must not exceed 100 characters");

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters");

        RuleFor(x => x.AdminFirstName)
            .NotEmpty().WithMessage("Admin first name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.AdminLastName)
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");
    }
}
