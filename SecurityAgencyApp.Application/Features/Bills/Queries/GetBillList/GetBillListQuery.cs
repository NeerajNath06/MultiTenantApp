using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Bills.Queries.GetBillList;

public class GetBillListQuery : IRequest<ApiResponse<BillListResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}

public class BillListResponseDto
{
    public List<BillDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class BillDto
{
    public Guid Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public Guid? SiteId { get; set; }
    public string? SiteName { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
