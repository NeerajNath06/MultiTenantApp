using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Attendance.Commands.CheckIn;
using SecurityAgencyApp.Application.Features.Attendance.Commands.CheckOut;
using SecurityAgencyApp.Application.Features.Attendance.Commands.MarkAttendance;
using SecurityAgencyApp.Application.Features.Attendance.Queries.GetAttendanceList;
using SecurityAgencyApp.Application.Features.Attendance.Queries.GetGuardAttendanceByDate;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AttendanceListResponseDto>>> GetAttendanceList(
        [FromQuery] Guid? guardId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetAttendanceListQuery
        {
            GuardId = guardId,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("mark")]
    public async Task<ActionResult<ApiResponse<Guid>>> MarkAttendance([FromBody] MarkAttendanceCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("check-in")]
    public async Task<ActionResult<ApiResponse<CheckInResultDto>>> CheckIn([FromBody] CheckInCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    /// <summary>
    /// Guard check-out (geofencing: only allowed within allocated site radius).
    /// </summary>
    [HttpPost("check-out")]
    public async Task<ActionResult<ApiResponse<CheckOutResultDto>>> CheckOut([FromBody] CheckOutCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    /// <summary>
    /// Get guard attendance for a date (for mobile: find current check-in without check-out).
    /// </summary>
    [HttpGet("guard/{guardId}")]
    public async Task<ActionResult<ApiResponse<List<GuardAttendanceItemDto>>>> GetGuardAttendance(
        Guid guardId,
        [FromQuery] DateTime? date = null)
    {
        var query = new GetGuardAttendanceByDateQuery { GuardId = guardId, Date = date };
        var result = await _mediator.Send(query);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }
}
