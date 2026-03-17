using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class CreateClientRequest
{
    [Required]
    [Display(Name = "Client Code")]
    public string ClientCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [Phone]
    public string? AlternatePhone { get; set; }

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Pin Code")]
    public string PinCode { get; set; } = string.Empty;
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPinCode { get; set; }
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? Website { get; set; }
    public string? AccountManagerName { get; set; }
    public string? BillingContactName { get; set; }

    [EmailAddress]
    public string? BillingContactEmail { get; set; }
    public string? EscalationContactName { get; set; }

    [EmailAddress]
    public string? EscalationContactEmail { get; set; }

    [Range(0, 3650)]
    public int? CreditPeriodDays { get; set; }
    public string? BillingCycle { get; set; }
    public string? GstState { get; set; }
    public string? PaymentModePreference { get; set; }
    public string? TaxTreatment { get; set; }
    public string? InvoicePrefix { get; set; }
    public string? SlaTerms { get; set; }

    [Range(0, 720)]
    public int? EscalationTatHours { get; set; }
    public string? PenaltyTerms { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}
