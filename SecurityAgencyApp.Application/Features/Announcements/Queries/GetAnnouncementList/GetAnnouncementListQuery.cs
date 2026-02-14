using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Announcements.Queries.GetAnnouncementList;

public class GetAnnouncementListQuery : IRequest<ApiResponse<AnnouncementListResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public string? Category { get; set; }
    public bool? IsPinned { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}

public class AnnouncementListResponseDto
{
    public List<AnnouncementDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AnnouncementDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? PostedByName { get; set; }
    public DateTime PostedAt { get; set; }
    public bool IsPinned { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
