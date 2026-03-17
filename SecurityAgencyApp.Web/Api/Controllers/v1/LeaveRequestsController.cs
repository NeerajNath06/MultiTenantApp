using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.LeaveRequests.Commands.ApproveLeaveRequest;
using SecurityAgencyApp.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;
using SecurityAgencyApp.Application.Features.LeaveRequests.Commands.DeleteLeaveRequest;
using SecurityAgencyApp.Application.Features.LeaveRequests.Queries.GetLeaveRequestById;
using SecurityAgencyApp.Application.Features.LeaveRequests.Queries.GetLeaveRequestList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class LeaveRequestsController : GenericReadCreateDeleteControllerBase<
    LeaveRequestListResponseDto,
    LeaveRequestDetailDto,
    GetLeaveRequestListQuery,
    GetLeaveRequestByIdQuery,
    CreateLeaveRequestCommand,
    DeleteLeaveRequestCommand>
{
    private readonly IMediator _mediator;

    public LeaveRequestsController(IMediator mediator) : base(mediator)
    {
        _mediator = mediator;
    }

    protected override GetLeaveRequestByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override DeleteLeaveRequestCommand CreateDeleteCommand(Guid id) => new() { Id = id };

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<SecurityAgencyApp.Application.Common.Models.ApiResponse<bool>>> ApproveLeaveRequest(Guid id, [FromBody] ApproveLeaveRequestCommand command)
    {
        command.LeaveRequestId = id;
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }
}
