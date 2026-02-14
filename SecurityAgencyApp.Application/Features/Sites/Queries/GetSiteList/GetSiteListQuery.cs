using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Sites.Queries.GetSiteList;

public class GetSiteListQuery : IRequest<ApiResponse<SiteListResponseDto>>
{
    public bool IncludeInactive { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
    /// <summary>When set, return only sites this supervisor is assigned to (SiteSupervisor).</summary>
    public Guid? SupervisorId { get; set; }
}

public class SiteListResponseDto
{
    public List<SiteDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? Search { get; set; }
}

public class SiteDto
{
    public Guid Id { get; set; }
    public string SiteCode { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public int GuardCount { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
