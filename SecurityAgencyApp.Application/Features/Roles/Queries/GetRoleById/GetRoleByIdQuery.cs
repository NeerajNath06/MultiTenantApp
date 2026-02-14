using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQuery : IRequest<ApiResponse<RoleDto>>
{
    public Guid Id { get; set; }
}

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
    public List<Guid> MenuIds { get; set; } = new();
    public List<Guid> SubMenuIds { get; set; } = new();
}
