using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Designations.Commands.CreateDesignation;
using SecurityAgencyApp.Application.Features.Designations.Commands.DeleteDesignation;
using SecurityAgencyApp.Application.Features.Designations.Commands.UpdateDesignation;
using SecurityAgencyApp.Application.Features.Designations.Queries.GetDesignationById;
using SecurityAgencyApp.Application.Features.Designations.Queries.GetDesignationList;
using GetDesignationById = SecurityAgencyApp.Application.Features.Designations.Queries.GetDesignationById;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class DesignationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DesignationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all designations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<DesignationListResponseDto>>> GetDesignations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] Guid? departmentId = null)
    {
        var query = new GetDesignationListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncludeInactive = includeInactive,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            DepartmentId = departmentId
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get designation by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetDesignationById.DesignationDto>>> GetDesignationById(Guid id)
    {
        var query = new GetDesignationByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (result.Success)
        {
            return Ok(result);
        }
        return NotFound(result);
    }

    /// <summary>
    /// Create a new designation
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateDesignation([FromBody] CreateDesignationCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetDesignationById), new { id = result.Data }, result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Update designation
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateDesignation(Guid id, [FromBody] UpdateDesignationCommand command)
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
    /// Delete designation
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDesignation(Guid id)
    {
        var command = new DeleteDesignationCommand { Id = id };
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
