using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
