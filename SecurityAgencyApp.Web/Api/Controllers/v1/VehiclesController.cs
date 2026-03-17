using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.VehicleLogs.Commands.RecordVehicleExit;
using SecurityAgencyApp.Application.Features.VehicleLogs.Commands.DeleteVehicleLog;
using SecurityAgencyApp.Application.Features.VehicleLogs.Commands.RegisterVehicleEntry;
using SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogById;
using SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogList;
using SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogSummaryBySite;
using VehicleLogByIdDto = SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogById.VehicleLogDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
[Authorize]
public class VehiclesController : GenericDetailedReadCreateDeleteControllerBase<
    VehicleLogListResponseDto,
    VehicleLogByIdDto,
    GetVehicleLogListQuery,
    GetVehicleLogByIdQuery,
    RegisterVehicleEntryCommand,
    RegisterVehicleEntryResultDto,
    DeleteVehicleLogCommand>
{
    private readonly IMediator _mediator;

    public VehiclesController(IMediator mediator) : base(mediator)
    {
        _mediator = mediator;
    }

    protected override GetVehicleLogByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override DeleteVehicleLogCommand CreateDeleteCommand(Guid id) => new() { Id = id };

    protected override object GetCreatedRouteValues(RegisterVehicleEntryResultDto response) => new { id = response.Id };

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
    /// <summary>Record vehicle exit. Pass exitTime (UTC) or omit for now.</summary>
    [HttpPatch("{id}/exit")]
    public async Task<ActionResult<SecurityAgencyApp.Application.Common.Models.ApiResponse<bool>>> RecordExit(Guid id, [FromBody] RecordVehicleExitRequest body)
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
