using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.MonthlyDocuments.Commands.GenerateMonthlyDocuments;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class MonthlyDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MonthlyDocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Generate Bill + Wage for a site/month (Present-only). Returns IDs for tracking. Duplicates allowed.</summary>
    [HttpPost("generate-all")]
    public async Task<ActionResult<ApiResponse<GenerateMonthlyDocumentsResultDto>>> GenerateAll([FromBody] GenerateMonthlyDocumentsCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}

