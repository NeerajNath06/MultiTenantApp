namespace SecurityAgencyApp.Model.Api;

public class CreateVehicleEntryRequest
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = "Car";
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? ParkingSlot { get; set; }
    public Guid SiteId { get; set; }
    public Guid GuardId { get; set; }
}
