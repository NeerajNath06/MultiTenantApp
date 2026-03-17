using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SecurityAgencyApp.Application.Features.Menus.Commands.AssignMenuToUser;
using SecurityAgencyApp.Application.Features.Roles.Commands.AssignMenusToRole;
using SecurityAgencyApp.Application.Features.Menus.Commands.CreateMenu;
using SecurityAgencyApp.Application.Features.Menus.Commands.DeleteMenu;
using SecurityAgencyApp.Application.Features.Menus.Commands.UpdateMenu;
using SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuList;
using SecurityAgencyApp.Application.Features.Menus.Queries.GetMenusForCurrentUser;
using SecurityAgencyApp.Application.Interfaces;
using GetMenuByIdQuery = SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuById.GetMenuByIdQuery;
using GetMenuByIdDto = SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuById.MenuDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class MenusController : GenericCrudControllerBase<
    MenuListResponseDto,
    GetMenuByIdDto,
    GetMenuListQuery,
    GetMenuByIdQuery,
    CreateMenuCommand,
    UpdateMenuCommand,
    DeleteMenuCommand>
{
    private readonly IMediator _mediator;
    private readonly IMemoryCache _cache;
    private readonly ICurrentUserService _currentUserService;

    public MenusController(IMediator mediator, IMemoryCache cache, ICurrentUserService currentUserService) : base(mediator)
    {
        _mediator = mediator;
        _cache = cache;
        _currentUserService = currentUserService;
    }

    protected override GetMenuByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateMenuCommand command, Guid id) => command.Id = id;

    protected override DeleteMenuCommand CreateDeleteCommand(Guid id) => new() { Id = id };

    /// <summary>
    /// Get menus and submenus allowed for the current user (by role). Use this for sidebar. Cached 5 min per user (enterprise).
    /// </summary>
    [HttpGet("for-current-user")]
    public async Task<ActionResult<SecurityAgencyApp.Application.Common.Models.ApiResponse<GetMenusForCurrentUserResponseDto>>> GetMenusForCurrentUser()
    {
        var userId = _currentUserService.UserId?.ToString() ?? "anon";
        var cacheKey = "menus:u:" + userId;
        if (_cache.TryGetValue(cacheKey, out GetMenusForCurrentUserResponseDto? cached))
            return Ok(SecurityAgencyApp.Application.Common.Models.ApiResponse<GetMenusForCurrentUserResponseDto>.SuccessResponse(cached!));
        var query = new GetMenusForCurrentUserQuery();
        var result = await _mediator.Send(query);
        if (result.Success && result.Data != null)
            _cache.Set(cacheKey, result.Data, TimeSpan.FromMinutes(5));
        return Ok(result);
    }

    /// <summary>
    /// Assign menus and submenus to role (department-wise; stored in DB).
    /// </summary>
    [HttpPost("{roleId}/assign")]
    public async Task<ActionResult<SecurityAgencyApp.Application.Common.Models.ApiResponse<bool>>> AssignMenusToRole(Guid roleId, [FromBody] AssignMenusToRoleRequest request)
    {
        var command = new AssignMenusToRoleCommand
        {
            RoleId = roleId,
            MenuIds = request?.MenuIds ?? new List<Guid>(),
            SubMenuIds = request?.SubMenuIds ?? new List<Guid>()
        };
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Assign menus to user
    /// </summary>
    [HttpPost("user/{userId}/assign")]
    public async Task<ActionResult<SecurityAgencyApp.Application.Common.Models.ApiResponse<bool>>> AssignMenuToUser(Guid userId, [FromBody] List<Guid> menuIds)
    {
        var command = new AssignMenuToUserCommand
        {
            UserId = userId,
            MenuIds = menuIds
        };
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

}

public class AssignMenusToRoleRequest
{
    public List<Guid> MenuIds { get; set; } = new();
    public List<Guid> SubMenuIds { get; set; } = new();
}
