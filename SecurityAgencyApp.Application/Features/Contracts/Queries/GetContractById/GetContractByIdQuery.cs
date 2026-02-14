using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Contracts.Queries.GetContractById;

public class GetContractByIdQuery : IRequest<ApiResponse<ContractDto>>
{
    public Guid Id { get; set; }
}

public class ContractDto
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public string BillingCycle { get; set; } = string.Empty;
    public decimal MonthlyAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TermsAndConditions { get; set; }
    public string? PaymentTerms { get; set; }
    public int? NumberOfGuards { get; set; }
    public bool AutoRenewal { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<ContractSiteDto> Sites { get; set; } = new();
}

public class ContractSiteDto
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public int RequiredGuards { get; set; }
    public string? ShiftDetails { get; set; }
}
