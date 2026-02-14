using FluentValidation;

namespace SecurityAgencyApp.Application.Features.TrainingRecords.Commands.CreateTrainingRecord;

public class CreateTrainingRecordCommandValidator : AbstractValidator<CreateTrainingRecordCommand>
{
    public CreateTrainingRecordCommandValidator()
    {
        RuleFor(x => x.GuardId)
            .NotEmpty().WithMessage("Guard is required");

        RuleFor(x => x.TrainingType)
            .NotEmpty().WithMessage("Training type is required")
            .MaximumLength(50).WithMessage("Training type must not exceed 50 characters");

        RuleFor(x => x.TrainingName)
            .NotEmpty().WithMessage("Training name is required")
            .MaximumLength(200).WithMessage("Training name must not exceed 200 characters");

        RuleFor(x => x.Score)
            .InclusiveBetween(0, 100).WithMessage("Score must be between 0 and 100")
            .When(x => x.Score.HasValue);
    }
}
