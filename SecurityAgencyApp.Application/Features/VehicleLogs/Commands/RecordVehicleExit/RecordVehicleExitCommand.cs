using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Commands.RecordVehicleExit;

public class RecordVehicleExitCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public DateTime ExitTime { get; set; }
}
