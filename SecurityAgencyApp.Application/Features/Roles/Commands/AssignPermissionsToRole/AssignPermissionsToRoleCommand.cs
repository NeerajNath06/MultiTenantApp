using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Roles.Commands.AssignPermissionsToRole;

public class AssignPermissionsToRoleCommand : IRequest<ApiResponse<bool>>
{
    public Guid RoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}
