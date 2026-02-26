using FluentValidation;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Commands.RegisterVehicleEntry;

public class RegisterVehicleEntryCommandValidator : AbstractValidator<RegisterVehicleEntryCommand>
{
    public RegisterVehicleEntryCommandValidator()
    {
        RuleFor(x => x.VehicleNumber).NotEmpty().WithMessage("Vehicle number is required").MaximumLength(30);
        RuleFor(x => x.DriverName).NotEmpty().WithMessage("Driver name is required").MaximumLength(200);
        RuleFor(x => x.SiteId).NotEmpty().WithMessage("Site is required");
        RuleFor(x => x.GuardId).NotEmpty().WithMessage("Guard is required");
        RuleFor(x => x.Purpose).MaximumLength(100);
        RuleFor(x => x.ParkingSlot).MaximumLength(20);
    }
}
