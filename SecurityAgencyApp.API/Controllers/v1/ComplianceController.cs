using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Compliance.Queries.GetComplianceSummary;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ComplianceController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComplianceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<ComplianceSummaryDto>>> GetSummary([FromQuery] Guid? supervisorId = null)
    {
        var query = new GetComplianceSummaryQuery { SupervisorId = supervisorId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
