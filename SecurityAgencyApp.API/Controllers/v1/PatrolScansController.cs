using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.PatrolScans.Commands.CreatePatrolScan;
using SecurityAgencyApp.Application.Features.PatrolScans.Queries.GetPatrolScanList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class PatrolScansController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatrolScansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PatrolScanListResponseDto>>> GetScans(
        [FromQuery] Guid guardId,
        [FromQuery] Guid? siteId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetPatrolScanListQuery
        {
            GuardId = guardId,
            SiteId = siteId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> RecordScan([FromBody] CreatePatrolScanCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success && result.Data != default)
            return CreatedAtAction(nameof(GetScans), new { guardId = command.GuardId }, result);
        return BadRequest(result);
    }
}
