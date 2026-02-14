using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Notifications.Commands.SendNotification;

public class SendNotificationCommand : IRequest<ApiResponse<SendNotificationResultDto>>
{
    /// <summary>Recipient user IDs (guards/supervisors/admins - each has a User record).</summary>
    public List<Guid> UserIds { get; set; } = new();
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = "Info"; // Info, Warning, Success, Error, Alert
}

public class SendNotificationResultDto
{
    public int SentCount { get; set; }
}
