using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Announcements.Commands.DeleteAnnouncement;

public class DeleteAnnouncementCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
