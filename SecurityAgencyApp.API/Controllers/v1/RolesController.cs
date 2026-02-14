using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Roles.Commands.AssignPermissionsToRole;
using SecurityAgencyApp.Application.Features.Roles.Commands.CreateRole;
using SecurityAgencyApp.Application.Features.Roles.Commands.DeleteRole;
using SecurityAgencyApp.Application.Features.Roles.Commands.UpdateRole;
using SecurityAgencyApp.Application.Features.Roles.Queries.GetRoleById;
using SecurityAgencyApp.Application.Features.Roles.Queries.GetRoleList;
using GetRoleByIdDto = SecurityAgencyApp.Application.Features.Roles.Queries.GetRoleById.RoleDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<RoleListResponseDto>>> GetRoles(
        [FromQuery] bool includeInactive = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        var query = new GetRoleListQuery
        {
            IncludeInactive = includeInactive,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetRoleByIdDto>>> GetRoleById(Guid id)
    {
        var result = await _mediator.Send(new GetRoleByIdQuery { Id = id });
        if (result.Success) return Ok(result);
        return NotFound(result);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateRole([FromBody] CreateRoleCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetRoles), new { id = result.Data }, result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Update role
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateRole(Guid id, [FromBody] UpdateRoleCommand command)
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
    /// Delete role
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRole(Guid id)
    {
        var command = new DeleteRoleCommand { Id = id };
        var result = await _mediator.Send(command);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("{roleId}/assign-permissions")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignPermissionsToRole(Guid roleId, [FromBody] List<Guid> permissionIds)
    {
        var command = new AssignPermissionsToRoleCommand { RoleId = roleId, PermissionIds = permissionIds ?? new List<Guid>() };
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }
}
