using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

/// <summary>
/// Vehicle entry/exit log at a site. Tenant and site scoped; recorded by guard.
/// </summary>
public class VehicleLog : TenantEntity
{
    public string VehicleNumber { get; set; } = string.Empty;
    /// <summary>Car, Bike, Truck, Auto, Other</summary>
    public string VehicleType { get; set; } = "Car";
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    /// <summary>Employee, Visitor, Delivery, Vendor, Client Meeting, etc.</summary>
    public string Purpose { get; set; } = string.Empty;
    public string? ParkingSlot { get; set; }
    public Guid SiteId { get; set; }
    public Guid GuardId { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Site Site { get; set; } = null!;
    public virtual SecurityGuard Guard { get; set; } = null!;
}
