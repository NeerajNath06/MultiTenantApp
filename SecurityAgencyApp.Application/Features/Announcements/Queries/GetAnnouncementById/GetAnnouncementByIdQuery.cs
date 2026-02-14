using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Announcements.Queries.GetAnnouncementById;

public class GetAnnouncementByIdQuery : IRequest<ApiResponse<AnnouncementDto>>
{
    public Guid Id { get; set; }
}

public class AnnouncementDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid? PostedByUserId { get; set; }
    public string? PostedByName { get; set; }
    public DateTime PostedAt { get; set; }
    public bool IsPinned { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
