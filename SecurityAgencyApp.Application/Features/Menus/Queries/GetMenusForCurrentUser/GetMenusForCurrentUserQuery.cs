using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuList;

namespace SecurityAgencyApp.Application.Features.Menus.Queries.GetMenusForCurrentUser;

/// <summary>
/// Returns menus and submenus that the current user is allowed to see (based on role assignments).
/// Used by Web sidebar – no hardcoding, everything from database.
/// </summary>
public class GetMenusForCurrentUserQuery : IRequest<ApiResponse<GetMenusForCurrentUserResponseDto>>
{
}

public class GetMenusForCurrentUserResponseDto
{
    public List<MenuDto> Items { get; set; } = new();
}
