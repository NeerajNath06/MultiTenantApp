using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class TenantProfileDto
{
    public Guid Id { get; set; }
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
    public string TimeZone { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string InvoicePrefix { get; set; } = string.Empty;
    public string PayrollPrefix { get; set; } = string.Empty;
    public string SubscriptionPlan { get; set; } = string.Empty;
    public int? SeatLimit { get; set; }
    public int? BranchLimit { get; set; }
    public decimal? StorageLimitGb { get; set; }
    public string OnboardingStatus { get; set; } = string.Empty;
    public string ActivationStatus { get; set; } = string.Empty;
    public bool IsKycVerified { get; set; }
    public bool OnboardingChecklistCompleted { get; set; }
    public string? LogoPath { get; set; }
    public bool IsActive { get; set; }
    public DateTime SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
}
