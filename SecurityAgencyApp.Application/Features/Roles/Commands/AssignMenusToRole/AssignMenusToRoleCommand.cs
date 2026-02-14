using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Roles.Commands.AssignMenusToRole;

public class AssignMenusToRoleCommand : IRequest<ApiResponse<bool>>
{
    public Guid RoleId { get; set; }
    public List<Guid> MenuIds { get; set; } = new();
    public List<Guid> SubMenuIds { get; set; } = new();
}
