using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.SiteRates.Commands.DeleteSiteRatePlan;
using SecurityAgencyApp.Application.Features.SiteRates.Commands.UpsertSiteRatePlan;
using SecurityAgencyApp.Application.Features.SiteRates.Queries.GetCurrentSiteRate;
using SecurityAgencyApp.Application.Features.SiteRates.Queries.GetSiteRateHistory;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SiteRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SiteRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get current per-day rate for a site (effective as-of date).</summary>
    [HttpGet("{siteId}")]
    public async Task<ActionResult<ApiResponse<SiteRateDto>>> GetCurrent(Guid siteId, [FromQuery] DateTime? asOfDate = null)
    {
        var result = await _mediator.Send(new GetCurrentSiteRateQuery { SiteId = siteId, AsOfDate = asOfDate });
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>Get rate plan history for a site (effective-dated).</summary>
    [HttpGet("{siteId}/history")]
    public async Task<ActionResult<ApiResponse<List<SiteRateHistoryDto>>>> GetHistory(Guid siteId, [FromQuery] bool includeInactive = true)
    {
        var result = await _mediator.Send(new GetSiteRateHistoryQuery { SiteId = siteId, IncludeInactive = includeInactive });
        return Ok(result);
    }

    /// <summary>Set per-day rate for a site (effective-dated). Pass Id to update existing; omit to create new.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Upsert([FromBody] UpsertSiteRatePlanCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Delete (deactivate) a rate plan.</summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteSiteRatePlanCommand { Id = id });
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}

