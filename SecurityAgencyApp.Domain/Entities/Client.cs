using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Client : TenantEntity
{
    public string ClientCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AlternatePhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? Website { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Suspended
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<Site> Sites { get; set; } = new List<Site>();
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
