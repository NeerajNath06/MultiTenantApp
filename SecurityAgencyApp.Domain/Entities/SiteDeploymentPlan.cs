using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class SiteDeploymentPlan : TenantEntity
{
    public Guid SiteId { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? ReservePoolMapping { get; set; }
    public string? AccessZones { get; set; }
    public string? EmergencyContactSet { get; set; }
    public string? InstructionSummary { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Site Site { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
}
