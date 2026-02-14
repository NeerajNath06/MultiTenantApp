using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Wages.Queries.GetWageList;

public class GetWageListQuery : IRequest<ApiResponse<WageListResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}

public class WageListResponseDto
{
    public List<WageDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class WageDto
{
    public Guid Id { get; set; }
    public string WageSheetNumber { get; set; } = string.Empty;
    public DateTime WagePeriodStart { get; set; }
    public DateTime WagePeriodEnd { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalWages { get; set; }
    public decimal NetAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
