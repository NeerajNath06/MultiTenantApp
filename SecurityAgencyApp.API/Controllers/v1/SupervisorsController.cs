using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Users.Queries.GetUserList;

namespace SecurityAgencyApp.API.Controllers.v1;

/// <summary>
/// Supervisors API – list users with Supervisor role for dropdowns and supervisor-based filtering.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SupervisorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupervisorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all users with Supervisor role (for assigning guards to supervisors).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<UserListResponseDto>>> GetSupervisors(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = true)
    {
        var query = new GetUserListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            IsActive = isActive,
            RoleCode = "SUPERVISOR"
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
