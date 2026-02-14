using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Menus.Commands.AssignMenuToUser;

public class AssignMenuToUserCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; set; }
    public List<Guid> MenuIds { get; set; } = new();
}
