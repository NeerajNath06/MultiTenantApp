using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.SecurityGuards.Commands.CreateGuard;
using SecurityAgencyApp.Application.Features.SecurityGuards.Commands.DeleteGuard;
using SecurityAgencyApp.Application.Features.SecurityGuards.Commands.UpdateGuard;
using SecurityAgencyApp.Application.Features.SecurityGuards.Queries.GetGuardList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class SecurityGuardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SecurityGuardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all security guards
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<GuardListResponseDto>>> GetGuards(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] Guid? supervisorId = null)
    {
        var query = new GetGuardListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncludeInactive = includeInactive,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            SupervisorId = supervisorId
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get security guard by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SecurityAgencyApp.Application.Features.SecurityGuards.Queries.GetGuardById.GuardDto>>> GetGuardById(Guid id)
    {
        var query = new SecurityAgencyApp.Application.Features.SecurityGuards.Queries.GetGuardById.GetGuardByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (result.Success)
        {
            return Ok(result);
        }
        return NotFound(result);
    }

    /// <summary>
    /// Create a new security guard
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateGuard([FromBody] CreateGuardCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetGuards), new { id = result.Data }, result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Update security guard
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateGuard(Guid id, [FromBody] UpdateGuardCommand command)
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
    /// Delete security guard
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteGuard(Guid id)
    {
        var command = new DeleteGuardCommand { Id = id };
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
