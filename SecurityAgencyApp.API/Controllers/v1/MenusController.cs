using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Menus.Commands.AssignMenuToUser;
using SecurityAgencyApp.Application.Features.Roles.Commands.AssignMenusToRole;
using SecurityAgencyApp.Application.Features.Menus.Commands.CreateMenu;
using SecurityAgencyApp.Application.Features.Menus.Commands.UpdateMenu;
using SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuList;
using SecurityAgencyApp.Application.Features.Menus.Queries.GetMenusForCurrentUser;
using SecurityAgencyApp.Application.Interfaces;
using GetMenuByIdQuery = SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuById.GetMenuByIdQuery;
using GetMenuByIdDto = SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuById.MenuDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class MenusController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMemoryCache _cache;
    private readonly ICurrentUserService _currentUserService;

    public MenusController(IMediator mediator, IMemoryCache cache, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _cache = cache;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get all menus
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<MenuListResponseDto>>> GetMenus([FromQuery] bool includeInactive = false, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = "asc")
    {
        var query = new GetMenuListQuery { IncludeInactive = includeInactive, PageNumber = pageNumber, PageSize = pageSize, Search = search, SortBy = sortBy, SortDirection = sortDirection };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get menus and submenus allowed for the current user (by role). Use this for sidebar. Cached 5 min per user (enterprise).
    /// </summary>
    [HttpGet("for-current-user")]
    public async Task<ActionResult<ApiResponse<GetMenusForCurrentUserResponseDto>>> GetMenusForCurrentUser()
    {
        var userId = _currentUserService.UserId?.ToString() ?? "anon";
        var cacheKey = "menus:u:" + userId;
        if (_cache.TryGetValue(cacheKey, out GetMenusForCurrentUserResponseDto? cached))
            return Ok(ApiResponse<GetMenusForCurrentUserResponseDto>.SuccessResponse(cached!, "Menus retrieved (cached)"));
        var query = new GetMenusForCurrentUserQuery();
        var result = await _mediator.Send(query);
        if (result.Success && result.Data != null)
            _cache.Set(cacheKey, result.Data, TimeSpan.FromMinutes(5));
        return Ok(result);
    }

    /// <summary>
    /// Get menu by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetMenuByIdDto>>> GetMenuById(Guid id)
    {
        var query = new GetMenuByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (result.Success)
        {
            return Ok(result);
        }
        return NotFound(result);
    }

    /// <summary>
    /// Create a new menu
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateMenu([FromBody] CreateMenuCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetMenuById), new { id = result.Data }, result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Update menu
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMenu(Guid id, [FromBody] UpdateMenuCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Assign menus and submenus to role (department-wise; stored in DB).
    /// </summary>
    [HttpPost("{roleId}/assign")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignMenusToRole(Guid roleId, [FromBody] AssignMenusToRoleRequest request)
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
    public async Task<ActionResult<ApiResponse<bool>>> AssignMenuToUser(Guid userId, [FromBody] List<Guid> menuIds)
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

    /// <summary>
    /// Delete menu
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMenu(Guid id)
    {
        var command = new SecurityAgencyApp.Application.Features.Menus.Commands.DeleteMenu.DeleteMenuCommand { Id = id };
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
