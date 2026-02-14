using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuById;

public class GetMenuByIdQuery : IRequest<ApiResponse<MenuDto>>
{
    public Guid Id { get; set; }
}

public class MenuDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<SubMenuDto> SubMenus { get; set; } = new();
    public List<Guid> PermissionIds { get; set; } = new();
}

public class SubMenuDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
