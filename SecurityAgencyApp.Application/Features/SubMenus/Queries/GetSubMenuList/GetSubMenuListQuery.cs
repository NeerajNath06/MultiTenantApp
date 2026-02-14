using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SubMenus.Queries.GetSubMenuList;

public class GetSubMenuListQuery : IRequest<ApiResponse<SubMenuListResponseDto>>
{
    public Guid? MenuId { get; set; }
    public bool IncludeInactive { get; set; } = false;
}

public class SubMenuListResponseDto
{
    public List<SubMenuDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class SubMenuDto
{
    public Guid Id { get; set; }
    public Guid MenuId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
