using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Wages.Commands.CreateWage;
using SecurityAgencyApp.Application.Features.Wages.Queries.GetWageDetailsByGuard;
using SecurityAgencyApp.Application.Features.Wages.Queries.GetWageList;
using SecurityAgencyApp.Application.Features.Wages.Queries.GetWageWithDetails;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class WagesController : GenericReadCreateControllerBase<
    WageListResponseDto,
    WageWithDetailsDto,
    GetWageListQuery,
    GetWageWithDetailsQuery,
    CreateWageCommand>
{
    private readonly IMediator _mediator;

    public WagesController(IMediator mediator) : base(mediator)
    {
        _mediator = mediator;
    }

    protected override GetWageWithDetailsQuery CreateGetByIdQuery(Guid id) => new() { WageId = id };

    [HttpGet("guard/{guardId}")]
    public async Task<ActionResult<SecurityAgencyApp.Application.Common.Models.ApiResponse<GuardPayslipsResponseDto>>> GetGuardPayslips(
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
}
