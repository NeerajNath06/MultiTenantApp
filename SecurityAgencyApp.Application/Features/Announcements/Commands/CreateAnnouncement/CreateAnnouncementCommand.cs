using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Announcements.Commands.CreateAnnouncement;

public class CreateAnnouncementCommand : IRequest<ApiResponse<Guid>>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
    public Guid? PostedByUserId { get; set; }
    public string? PostedByName { get; set; }
    public bool IsPinned { get; set; }
}
