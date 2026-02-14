using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Wages.Commands.CreateWage;
using SecurityAgencyApp.Application.Features.Wages.Queries.GetWageDetailsByGuard;
using SecurityAgencyApp.Application.Features.Wages.Queries.GetWageList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class WagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("guard/{guardId}")]
    public async Task<ActionResult<ApiResponse<GuardPayslipsResponseDto>>> GetGuardPayslips(
        Guid guardId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 24,
        [FromQuery] DateTime? periodStart = null,
        [FromQuery] DateTime? periodEnd = null)
    {
        var query = new GetWageDetailsByGuardQuery
        {
            GuardId = guardId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };
        var result = await _mediator.Send(query);
        if (result.Success && result.Data != null) return Ok(result);
        return NotFound(result);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<WageListResponseDto>>> GetWages(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? periodStart = null,
        [FromQuery] DateTime? periodEnd = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetWageListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            Status = status,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            IncludeInactive = includeInactive,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateWage([FromBody] CreateWageCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return CreatedAtAction(nameof(GetWages), new { id = result.Data }, result);
        return BadRequest(result);
    }
}
