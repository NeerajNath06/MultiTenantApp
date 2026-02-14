using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Users.Commands.CreateUser;
using SecurityAgencyApp.Application.Features.Users.Queries.GetUserList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get users list
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<UserListResponseDto>>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? designationId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? roleCode = null)
    {
        var query = new GetUserListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            DepartmentId = departmentId,
            DesignationId = designationId,
            IsActive = isActive,
            RoleCode = roleCode
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get user by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SecurityAgencyApp.Application.Features.Users.Queries.GetUserById.UserDto>>> GetUserById(Guid id)
    {
        var query = new SecurityAgencyApp.Application.Features.Users.Queries.GetUserById.GetUserByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (result.Success)
        {
            return Ok(result);
        }
        return NotFound(result);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetUserById), new { id = result.Data }, result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateUser(Guid id, [FromBody] SecurityAgencyApp.Application.Features.Users.Commands.UpdateUser.UpdateUserCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
