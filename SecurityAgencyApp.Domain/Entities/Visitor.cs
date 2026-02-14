using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Visitor : TenantEntity
{
    public string VisitorName { get; set; } = string.Empty;
    public string VisitorType { get; set; } = "Individual"; // Individual, Vendor, Delivery, etc.
    public string? CompanyName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Purpose { get; set; } = string.Empty; // Meeting, Interview, Delivery, etc.
    public string? HostName { get; set; }
    public string? HostDepartment { get; set; }
    public Guid SiteId { get; set; }
    public Guid GuardId { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public string? IdProofType { get; set; }
    public string? IdProofNumber { get; set; }
    public string? BadgeNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Site Site { get; set; } = null!;
    public virtual SecurityGuard Guard { get; set; } = null!;
}
