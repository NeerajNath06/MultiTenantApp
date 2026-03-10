using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Commands.DeleteVehicleLog;

public class DeleteVehicleLogCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
