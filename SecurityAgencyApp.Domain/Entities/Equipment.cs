using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Equipment : TenantEntity
{
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Uniform, Weapon, Communication, Vehicle, Other
    public string? Manufacturer { get; set; }
    public string? ModelNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public string Status { get; set; } = "Available"; // Available, Assigned, Maintenance, Damaged, Retired
    public Guid? AssignedToGuardId { get; set; }
    public Guid? AssignedToSiteId { get; set; }
    public DateTime? AssignedDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual SecurityGuard? AssignedToGuard { get; set; }
    public virtual Site? AssignedToSite { get; set; }
}
