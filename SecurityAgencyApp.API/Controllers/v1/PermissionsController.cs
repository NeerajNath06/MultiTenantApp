using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Permissions.Queries.GetPermissionList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PermissionListResponseDto>>> GetPermissions([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetPermissionListQuery { IncludeInactive = includeInactive });
        return Ok(result);
    }
}
