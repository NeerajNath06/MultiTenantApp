using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Contracts.Commands.UpdateContract;

public class UpdateContractCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public string BillingCycle { get; set; } = "Monthly";
    public decimal MonthlyAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public string? TermsAndConditions { get; set; }
    public string? PaymentTerms { get; set; }
    public int? NumberOfGuards { get; set; }
    public bool AutoRenewal { get; set; } = false;
    public DateTime? RenewalDate { get; set; }
    public string? Notes { get; set; }
}
