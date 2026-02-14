namespace SecurityAgencyApp.Web.Models.Api;

public class ContractListResponse
{
    public List<ContractItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ContractItemDto
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public decimal MonthlyAmount { get; set; }
    public string BillingCycle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class ContractDetailDto
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
    public List<ContractSiteItemDto> Sites { get; set; } = new();
}

public class ContractSiteItemDto
{
    public Guid SiteId { get; set; }
    public string? SiteName { get; set; }
    public int RequiredGuards { get; set; }
    public string? ShiftDetails { get; set; }
}

public class CreateContractRequest
{
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
    public bool AutoRenewal { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string? Notes { get; set; }
    public List<ContractSiteItemDto> Sites { get; set; } = new();
}

public class UpdateContractRequest
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
    public bool AutoRenewal { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string? Notes { get; set; }
}
