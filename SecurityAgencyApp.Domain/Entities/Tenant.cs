using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Tenant : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TradeName { get; set; }
    public string? CompanyCode { get; set; }
    public string? CinNumber { get; set; }
    public string? GstNumber { get; set; }
    public string? PanNumber { get; set; }
    public string? PfNumber { get; set; }
    public string? EsicNumber { get; set; }
    public string? LabourLicenseNumber { get; set; }
    public string? OwnerName { get; set; }
    public string? ComplianceOfficerName { get; set; }
    public string? BillingContactName { get; set; }
    public string? BillingContactPhone { get; set; }
    public string? BillingEmail { get; set; }
    public string? EscalationContactName { get; set; }
    public string? EscalationContactPhone { get; set; }
    public string? SupportEmail { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PinCode { get; set; }
    public string? Website { get; set; }
    public string? TaxId { get; set; }
    public string TimeZone { get; set; } = "Asia/Kolkata";
    public string Currency { get; set; } = "INR";
    public string InvoicePrefix { get; set; } = "INV";
    public string PayrollPrefix { get; set; } = "PAY";
    public string SubscriptionPlan { get; set; } = "Standard";
    public int? SeatLimit { get; set; }
    public int? BranchLimit { get; set; }
    public decimal? StorageLimitGb { get; set; }
    public string OnboardingStatus { get; set; } = "Pending";
    public string ActivationStatus { get; set; } = "Draft";
    public bool IsKycVerified { get; set; }
    public bool OnboardingChecklistCompleted { get; set; }
    public string? LogoPath { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<TenantDocument> Documents { get; set; } = new List<TenantDocument>();
}
