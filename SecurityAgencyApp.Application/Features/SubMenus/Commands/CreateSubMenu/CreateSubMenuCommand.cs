using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SubMenus.Commands.CreateSubMenu;

public class CreateSubMenuCommand : IRequest<ApiResponse<Guid>>
{
    public Guid MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid>? PermissionIds { get; set; }
}
