using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class SiteRatePlan : TenantEntity
{
    public Guid ClientId { get; set; }
    public Guid SiteId { get; set; }
    public decimal RateAmount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public decimal? AllowancePercent { get; set; }
    public decimal? EpfPercent { get; set; }
    public decimal? EsicPercent { get; set; }
    public decimal? EpfWageCap { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Client Client { get; set; } = null!;
    public virtual Site Site { get; set; } = null!;
}
