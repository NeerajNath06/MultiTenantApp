using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Visitors.Commands.CreateVisitor;
using SecurityAgencyApp.Application.Features.Visitors.Commands.UpdateVisitorExit;
using SecurityAgencyApp.Application.Features.Visitors.Commands.UpdateVisitor;
using SecurityAgencyApp.Application.Features.Visitors.Commands.DeleteVisitor;
using SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorAnalytics;
using SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorById;
using SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorList;
using VisitorByIdDto = SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorById.VisitorDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class VisitorsController : GenericDetailedCrudControllerBase<
    VisitorListResponseDto,
    VisitorByIdDto,
    GetVisitorListQuery,
    GetVisitorByIdQuery,
    CreateVisitorCommand,
    CreateVisitorResultDto,
    UpdateVisitorCommand,
    DeleteVisitorCommand>
{
    private readonly IMediator _mediator;

    public VisitorsController(IMediator mediator) : base(mediator)
    {
        _mediator = mediator;
    }

    protected override GetVisitorByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateVisitorCommand command, Guid id) => command.Id = id;

    protected override DeleteVisitorCommand CreateDeleteCommand(Guid id) => new() { Id = id };

    protected override object GetCreatedRouteValues(CreateVisitorResultDto response) => new { id = response.Id };

    [HttpGet("analytics")]
    public async Task<ActionResult<SecurityAgencyApp.Application.Common.Models.ApiResponse<VisitorAnalyticsDto>>> GetAnalytics(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? supervisorId = null)
    {
        var analyticsQuery = new GetVisitorAnalyticsQuery
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            SiteId = siteId,
            SupervisorId = supervisorId
        };
        var result = await _mediator.Send(analyticsQuery);
        return Ok(result);
    }

    [HttpPatch("{id}/exit")]
    public async Task<ActionResult<SecurityAgencyApp.Application.Common.Models.ApiResponse<bool>>> RecordExit(Guid id, [FromBody] RecordExitRequest body)
    {
        var command = new UpdateVisitorExitCommand
        {
            Id = id,
            ExitTime = body.ExitTime ?? DateTime.UtcNow
        };
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return NotFound(result);
    }
}

public class RecordExitRequest
{
    public DateTime? ExitTime { get; set; }
}
