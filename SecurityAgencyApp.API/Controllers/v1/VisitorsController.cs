using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Visitors.Commands.CreateVisitor;
using SecurityAgencyApp.Application.Features.Visitors.Commands.UpdateVisitorExit;
using SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorAnalytics;
using SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorById;
using SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorList;
using VisitorByIdDto = SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorById.VisitorDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class VisitorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public VisitorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<VisitorListResponseDto>>> GetVisitors(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? guardId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] bool? insideOnly = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var listQuery = new GetVisitorListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            SiteId = siteId,
            GuardId = guardId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            InsideOnly = insideOnly,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(listQuery);
        return Ok(result);
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<VisitorAnalyticsDto>>> GetAnalytics(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? supervisorId = null)
    {
        var analyticsQuery = new GetVisitorAnalyticsQuery
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            SiteId = siteId,
            SupervisorId = supervisorId
        };
        var result = await _mediator.Send(analyticsQuery);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VisitorByIdDto>>> GetVisitorById(Guid id)
    {
        var result = await _mediator.Send(new GetVisitorByIdQuery { Id = id });
        if (result.Success && result.Data != null) return Ok(result);
        return NotFound(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateVisitorResultDto>>> CreateVisitor([FromBody] CreateVisitorCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success && result.Data != null)
            return CreatedAtAction(nameof(GetVisitorById), new { id = result.Data.Id }, result);
        return BadRequest(result);
    }

    [HttpPatch("{id}/exit")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordExit(Guid id, [FromBody] RecordExitRequest body)
    {
        var command = new UpdateVisitorExitCommand
        {
            Id = id,
            ExitTime = body.ExitTime ?? DateTime.UtcNow
        };
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return NotFound(result);
    }
}

public class RecordExitRequest
{
    public DateTime? ExitTime { get; set; }
}
