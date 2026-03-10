using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

/// <summary>
/// Site billing rate plan (effective-dated). Only Present days are billable/payable as per policy.
/// </summary>
public class SiteRatePlan : TenantEntity
{
    public Guid ClientId { get; set; }
    public Guid SiteId { get; set; }
    /// <summary>Per-day rate amount used for billing/wages.</summary>
    public decimal RateAmount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>EPF deduction percentage (e.g. 12). Used for wages sheet.</summary>
    public decimal? EpfPercent { get; set; }
    /// <summary>ESIC deduction percentage (e.g. 0.75). Used for wages sheet.</summary>
    public decimal? EsicPercent { get; set; }
    /// <summary>Allowance percentage (e.g. 0). Used for wages sheet.</summary>
    public decimal? AllowancePercent { get; set; }
    /// <summary>EPF wage cap (e.g. 15000). Wages above this use this amount for EPF calculation.</summary>
    public decimal? EpfWageCap { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Client Client { get; set; } = null!;
    public virtual Site Site { get; set; } = null!;
}

