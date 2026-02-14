using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Shifts.Commands.CreateShift;
using SecurityAgencyApp.Application.Features.Shifts.Commands.DeleteShift;
using SecurityAgencyApp.Application.Features.Shifts.Commands.UpdateShift;
using SecurityAgencyApp.Application.Features.Shifts.Queries.GetShiftById;
using SecurityAgencyApp.Application.Features.Shifts.Queries.GetShiftList;
using GetShiftById = SecurityAgencyApp.Application.Features.Shifts.Queries.GetShiftById;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShiftsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all shifts
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<ShiftListResponseDto>>> GetShifts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        var query = new GetShiftListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncludeInactive = includeInactive,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get shift by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetShiftById.ShiftDto>>> GetShiftById(Guid id)
    {
        var query = new GetShiftByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (result.Success)
        {
            return Ok(result);
        }
        return NotFound(result);
    }

    /// <summary>
    /// Create a new shift
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateShift([FromBody] CreateShiftCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetShiftById), new { id = result.Data }, result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Update shift
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateShift(Guid id, [FromBody] UpdateShiftCommand command)
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
    /// Delete shift
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteShift(Guid id)
    {
        var command = new DeleteShiftCommand { Id = id };
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
