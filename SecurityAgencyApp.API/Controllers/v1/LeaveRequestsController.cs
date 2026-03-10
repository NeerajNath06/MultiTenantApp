using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.LeaveRequests.Commands.ApproveLeaveRequest;
using SecurityAgencyApp.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;
using SecurityAgencyApp.Application.Features.LeaveRequests.Commands.DeleteLeaveRequest;
using SecurityAgencyApp.Application.Features.LeaveRequests.Queries.GetLeaveRequestList;
using SecurityAgencyApp.Application.Features.LeaveRequests.Queries.GetLeaveRequestById;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class LeaveRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaveRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<LeaveRequestListResponseDto>>> GetLeaveRequests(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? guardId = null,
        [FromQuery] Guid? supervisorId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? leaveType = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetLeaveRequestListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            GuardId = guardId,
            SupervisorId = supervisorId,
            Status = status,
            LeaveType = leaveType,
            IncludeInactive = includeInactive,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateLeaveRequest([FromBody] CreateLeaveRequestCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return CreatedAtAction(nameof(GetLeaveRequests), new { id = result.Data }, result);
        return BadRequest(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveLeaveRequest(Guid id, [FromBody] ApproveLeaveRequestCommand command)
    {
        command.LeaveRequestId = id;
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDetailDto>>> GetLeaveRequestById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLeaveRequestByIdQuery { Id = id }, cancellationToken);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteLeaveRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteLeaveRequestCommand { Id = id }, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
