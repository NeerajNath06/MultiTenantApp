using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Wage : TenantEntity
{
    public string WageSheetNumber { get; set; } = string.Empty;
    public DateTime WagePeriodStart { get; set; }
    public DateTime WagePeriodEnd { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Approved, Paid, Cancelled
    public decimal TotalWages { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal NetAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<WageDetail> WageDetails { get; set; } = new List<WageDetail>();
}

public class WageDetail : BaseEntity
{
    public Guid WageId { get; set; }
    public Guid GuardId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? ShiftId { get; set; }
    public int DaysWorked { get; set; }
    public int HoursWorked { get; set; }
    public decimal BasicRate { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeRate { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string? Remarks { get; set; }

    // Navigation properties
    public virtual Wage Wage { get; set; } = null!;
    public virtual SecurityGuard Guard { get; set; } = null!;
    public virtual Site? Site { get; set; }
    public virtual Shift? Shift { get; set; }
}
