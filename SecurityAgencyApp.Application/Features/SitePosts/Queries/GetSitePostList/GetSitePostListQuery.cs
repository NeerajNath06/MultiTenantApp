using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.SitePosts;

namespace SecurityAgencyApp.Application.Features.SitePosts.Queries.GetSitePostList;

public class GetSitePostListQuery : IRequest<ApiResponse<SitePostListResponseDto>>
{
    public bool IncludeInactive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? BranchId { get; set; }
}

public class SitePostListResponseDto
{
    public List<SitePostDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? BranchId { get; set; }
}
