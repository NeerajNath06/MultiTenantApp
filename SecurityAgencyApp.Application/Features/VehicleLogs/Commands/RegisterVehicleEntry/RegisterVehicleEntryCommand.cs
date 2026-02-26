using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Commands.RegisterVehicleEntry;

public class RegisterVehicleEntryCommand : IRequest<ApiResponse<RegisterVehicleEntryResultDto>>
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = "Car"; // Car, Bike, Truck, Auto, Other
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    public string Purpose { get; set; } = string.Empty; // Employee, Visitor, Delivery, Vendor, etc.
    public string? ParkingSlot { get; set; }
    public Guid SiteId { get; set; }
    public Guid GuardId { get; set; }
}

public class RegisterVehicleEntryResultDto
{
    public Guid Id { get; set; }
    public DateTime EntryTime { get; set; }
}
