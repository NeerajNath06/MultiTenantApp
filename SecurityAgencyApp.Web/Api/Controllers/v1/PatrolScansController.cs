using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.PatrolScans.Commands.CreatePatrolScan;
using SecurityAgencyApp.Application.Features.PatrolScans.Queries.GetPatrolScanList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class PatrolScansController : GenericListCreateControllerBase<
    PatrolScanListResponseDto,
    GetPatrolScanListQuery,
    CreatePatrolScanCommand,
    Guid>
{
    public PatrolScansController(IMediator mediator) : base(mediator) { }

    protected override ActionResult<SecurityAgencyApp.Application.Common.Models.ApiResponse<Guid>> BuildCreateSuccess(CreatePatrolScanCommand command, SecurityAgencyApp.Application.Common.Models.ApiResponse<Guid> result)
        => CreatedAtAction(nameof(GetList), new { guardId = command.GuardId }, result);
}
