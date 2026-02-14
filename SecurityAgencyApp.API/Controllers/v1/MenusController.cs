using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Menus.Commands.AssignMenuToRole;
using SecurityAgencyApp.Application.Features.Menus.Commands.AssignMenuToUser;
using SecurityAgencyApp.Application.Features.Menus.Commands.CreateMenu;
using SecurityAgencyApp.Application.Features.Menus.Commands.UpdateMenu;
using SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuList;
using GetMenuByIdQuery = SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuById.GetMenuByIdQuery;
using GetMenuByIdDto = SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuById.MenuDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class MenusController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenusController(IMediator mediator)
    {
        _mediator = mediator;
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
    /// Assign menus to role
    /// </summary>
    [HttpPost("{roleId}/assign")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignMenuToRole(Guid roleId, [FromBody] List<Guid> menuIds)
    {
        var command = new AssignMenuToRoleCommand
        {
            RoleId = roleId,
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
