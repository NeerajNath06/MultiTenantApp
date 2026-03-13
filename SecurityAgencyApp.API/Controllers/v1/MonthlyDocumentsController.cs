using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.MonthlyDocuments.Commands.GenerateMonthlyDocuments;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class MonthlyDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MonthlyDocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("generate-all")]
    public async Task<ActionResult<ApiResponse<GenerateMonthlyDocumentsResultDto>>> GenerateAll([FromBody] GenerateMonthlyDocumentsCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
