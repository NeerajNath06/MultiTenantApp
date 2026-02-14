using FluentValidation;

namespace SecurityAgencyApp.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;

public class CreateLeaveRequestCommandValidator : AbstractValidator<CreateLeaveRequestCommand>
{
    public CreateLeaveRequestCommandValidator()
    {
        RuleFor(x => x.GuardId)
            .NotEmpty().WithMessage("Guard is required");

        RuleFor(x => x.LeaveType)
            .Must(type => new[] { "Casual", "Sick", "Emergency", "Annual", "Unpaid" }.Contains(type))
            .WithMessage("Invalid leave type");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}
