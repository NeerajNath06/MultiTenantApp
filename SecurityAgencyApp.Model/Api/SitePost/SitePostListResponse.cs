using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class SitePostListResponse
{
    public List<SitePostItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? BranchId { get; set; }
}
