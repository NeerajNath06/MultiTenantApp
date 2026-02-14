using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorList;

public class GetVisitorListQuery : IRequest<ApiResponse<VisitorListResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? GuardId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? InsideOnly { get; set; } // null = all, true = only those without ExitTime
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}

public class VisitorListResponseDto
{
    public List<VisitorDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class VisitorDto
{
    public Guid Id { get; set; }
    public string VisitorName { get; set; } = string.Empty;
    public string VisitorType { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? HostName { get; set; }
    public Guid SiteId { get; set; }
    public string? SiteName { get; set; }
    public Guid GuardId { get; set; }
    public string? GuardName { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public string? IdProofType { get; set; }
    public string? IdProofNumber { get; set; }
    public string? BadgeNumber { get; set; }
}
