using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class PatrolScan : TenantEntity
{
    public Guid GuardId { get; set; }
    public Guid SiteId { get; set; }
    public DateTime ScannedAt { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? CheckpointCode { get; set; }
    public string Status { get; set; } = "Success"; // Success, Failed, Pending

    public virtual Tenant Tenant { get; set; } = null!;
    public virtual SecurityGuard Guard { get; set; } = null!;
    public virtual Site Site { get; set; } = null!;
}
