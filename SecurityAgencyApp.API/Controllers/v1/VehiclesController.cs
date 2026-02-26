using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.VehicleLogs.Commands.RecordVehicleExit;
using SecurityAgencyApp.Application.Features.VehicleLogs.Commands.RegisterVehicleEntry;
using SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogById;
using SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogList;
using SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogSummaryBySite;
using VehicleLogByIdDto = SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogById.VehicleLogDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehiclesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get vehicle log summary by site (for admin/supervisor). Returns per-site counts: total, inside, exited.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<VehicleLogSummaryBySiteResponseDto>>> GetVehicleLogSummary(
        [FromQuery] Guid? siteId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var query = new GetVehicleLogSummaryBySiteQuery
        {
            SiteId = siteId,
            DateFrom = dateFrom,
            DateTo = dateTo
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>Get vehicle log list. Filter by siteId, guardId, date range, insideOnly (true = inside, false = exited, null = all). Guard: pass guardId+siteId to see only their site. Admin/Supervisor: pass only siteId to see all logs for that site.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<VehicleLogListResponseDto>>> GetVehicleLogs(
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
        var listQuery = new GetVehicleLogListQuery
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

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VehicleLogByIdDto>>> GetVehicleLogById(Guid id)
    {
        var result = await _mediator.Send(new GetVehicleLogByIdQuery { Id = id });
        if (result.Success && result.Data != null) return Ok(result);
        return NotFound(result);
    }

    /// <summary>Register vehicle entry (guard at site).</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RegisterVehicleEntryResultDto>>> RegisterVehicleEntry([FromBody] RegisterVehicleEntryCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success && result.Data != null)
            return CreatedAtAction(nameof(GetVehicleLogById), new { id = result.Data.Id }, result);
        return BadRequest(result);
    }

    /// <summary>Record vehicle exit. Pass exitTime (UTC) or omit for now.</summary>
    [HttpPatch("{id}/exit")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordExit(Guid id, [FromBody] RecordVehicleExitRequest body)
    {
        var command = new RecordVehicleExitCommand
        {
            Id = id,
            ExitTime = body.ExitTime ?? DateTime.UtcNow
        };
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return NotFound(result);
    }
}

public class RecordVehicleExitRequest
{
    public DateTime? ExitTime { get; set; }
}
