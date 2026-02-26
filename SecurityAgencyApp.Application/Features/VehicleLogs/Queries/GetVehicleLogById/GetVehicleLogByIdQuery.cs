using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogById;

public class GetVehicleLogByIdQuery : IRequest<ApiResponse<VehicleLogDto>>
{
    public Guid Id { get; set; }
}

public class VehicleLogDto
{
    public Guid Id { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? ParkingSlot { get; set; }
    public Guid SiteId { get; set; }
    public string? SiteName { get; set; }
    public Guid GuardId { get; set; }
    public string? GuardName { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
}
