using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Notifications.Commands.MarkAllAsRead;

public class MarkAllAsReadCommand : IRequest<ApiResponse<int>>
{
    public Guid UserId { get; set; }
}
