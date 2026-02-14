using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Contract : TenantEntity
{
    public string ContractNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public string BillingCycle { get; set; } = "Monthly"; // Monthly, Quarterly, Yearly, OneTime
    public decimal MonthlyAmount { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Active, Expired, Terminated, Renewed
    public string? TermsAndConditions { get; set; }
    public string? PaymentTerms { get; set; }
    public int? NumberOfGuards { get; set; }
    public bool AutoRenewal { get; set; } = false;
    public DateTime? RenewalDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Client Client { get; set; } = null!;
    public virtual ICollection<ContractSite> ContractSites { get; set; } = new List<ContractSite>();
}

public class ContractSite : BaseEntity
{
    public Guid ContractId { get; set; }
    public Guid SiteId { get; set; }
    public int RequiredGuards { get; set; }
    public string? ShiftDetails { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Contract Contract { get; set; } = null!;
    public virtual Site Site { get; set; } = null!;
}
