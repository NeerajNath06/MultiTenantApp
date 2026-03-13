using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class PayrollRun : TenantEntity
{
    public Guid? BranchId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? WageId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int PayrollYear { get; set; }
    public int PayrollMonth { get; set; }
    public string Status { get; set; } = "Draft";
    public int TotalGuards { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Notes { get; set; }

    public virtual Branch? Branch { get; set; }
    public virtual Site? Site { get; set; }
    public virtual Wage? Wage { get; set; }
}
