using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.GuardAssignments.Commands.CreateAssignment;
using SecurityAgencyApp.Application.Features.GuardAssignments.Commands.UpdateAssignment;
using SecurityAgencyApp.Application.Features.GuardAssignments.Queries.GetAssignmentList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class GuardAssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GuardAssignmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AssignmentListResponseDto>>> GetAssignments(
        [FromQuery] Guid? guardId = null,
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? supervisorId = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        var query = new GetAssignmentListQuery
        {
            GuardId = guardId,
            SiteId = siteId,
            SupervisorId = supervisorId,
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

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateAssignment([FromBody] CreateAssignmentCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return CreatedAtAction(nameof(GetAssignments), new { id = result.Data }, result);
        return BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAssignment(Guid id, [FromBody] UpdateAssignmentCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }
}
