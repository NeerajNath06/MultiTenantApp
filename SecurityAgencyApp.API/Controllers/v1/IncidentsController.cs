using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Incidents.Commands.CreateIncident;
using SecurityAgencyApp.Application.Features.Incidents.Commands.UpdateIncident;
using SecurityAgencyApp.Application.Features.Incidents.Queries.GetIncidentList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public IncidentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IncidentListResponseDto>>> GetIncidents(
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? guardId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? severity = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetIncidentListQuery
        {
            SiteId = siteId,
            GuardId = guardId,
            Status = status,
            Severity = severity,
            StartDate = startDate,
            EndDate = endDate,
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
    public async Task<ActionResult<ApiResponse<Guid>>> CreateIncident([FromBody] CreateIncidentCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return CreatedAtAction(nameof(GetIncidents), new { id = result.Data }, result);
        return BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateIncident(Guid id, [FromBody] UpdateIncidentCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }
}
