using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Menus.Commands.AssignMenuToRole;

public class AssignMenuToRoleCommand : IRequest<ApiResponse<bool>>
{
    public Guid RoleId { get; set; }
    public List<Guid> MenuIds { get; set; } = new();
}
