using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Dashboard.Queries.GetDashboardData;
using SecurityAgencyApp.Application.Features.Dashboard.Queries.GetMonthlySiteSummary;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardDataDto>>> GetDashboardData()
    {
        var result = await _mediator.Send(new GetDashboardDataQuery());
        return Ok(result);
    }

    /// <summary>Monthly summary per site (Present-only count, latest Bill total, latest Wage total) for a given month/year.</summary>
    [HttpGet("monthly-summary")]
    public async Task<ActionResult<ApiResponse<MonthlySiteSummaryResponseDto>>> GetMonthlySummary([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _mediator.Send(new GetMonthlySiteSummaryQuery { Year = year, Month = month });
        return Ok(result);
    }
}
