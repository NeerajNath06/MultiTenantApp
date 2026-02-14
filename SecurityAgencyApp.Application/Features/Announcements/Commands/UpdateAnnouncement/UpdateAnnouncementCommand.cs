using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Announcements.Commands.UpdateAnnouncement;

public class UpdateAnnouncementCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
    public bool IsPinned { get; set; }
    public bool IsActive { get; set; } = true;
}
