using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Clients.Queries.GetClientById;

public class GetClientByIdQuery : IRequest<ApiResponse<ClientDto>>
{
    public Guid Id { get; set; }
}

public class ClientDto
{
    public Guid Id { get; set; }
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
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPinCode { get; set; }
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? Website { get; set; }
    public string? AccountManagerName { get; set; }
    public string? BillingContactName { get; set; }
    public string? BillingContactEmail { get; set; }
    public string? EscalationContactName { get; set; }
    public string? EscalationContactEmail { get; set; }
    public int? CreditPeriodDays { get; set; }
    public string? BillingCycle { get; set; }
    public string? GstState { get; set; }
    public string? PaymentModePreference { get; set; }
    public string? TaxTreatment { get; set; }
    public string? InvoicePrefix { get; set; }
    public string? SlaTerms { get; set; }
    public int? EscalationTatHours { get; set; }
    public string? PenaltyTerms { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
