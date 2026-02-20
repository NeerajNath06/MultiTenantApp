using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.API.Services;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Attendance.Commands.CheckIn;
using SecurityAgencyApp.Application.Features.Attendance.Commands.CheckOut;
using SecurityAgencyApp.Application.Features.Attendance.Commands.MarkAttendance;
using SecurityAgencyApp.Application.Features.Attendance.Queries.GetAttendanceList;
using SecurityAgencyApp.Application.Features.Attendance.Queries.GetGuardAttendanceByDate;
using SecurityAgencyApp.Application.Features.TenantProfile.Queries.GetTenantProfile;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Export attendance as CSV, Excel (.xlsx), or PDF with enterprise header (agency name, address, guard details). format=csv|xlsx|pdf.</summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportAttendance(
        [FromQuery] string format = "csv",
        [FromQuery] Guid? guardId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var fmt = format?.Trim().ToLowerInvariant() ?? "csv";
        if (fmt != "csv" && fmt != "xlsx" && fmt != "pdf")
            return BadRequest("Supported format: csv, xlsx, pdf");

        var query = new GetAttendanceListQuery
        {
            GuardId = guardId,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            PageNumber = 1,
            PageSize = 50_000,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.Success || result.Data == null)
            return NotFound();
        var items = result.Data.Items;
        var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var profileResult = await _mediator.Send(new GetTenantProfileQuery(), cancellationToken);
        var header = ExportReportHeaderBuilder.BuildWithDateRange(profileResult.Data, "Attendance Report", startDate, endDate);

        if (fmt == "xlsx")
        {
            var bytes = ExportHelper.ToExcel("Attendance", "Attendance Report", items, header);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Attendance_{stamp}.xlsx");
        }
        if (fmt == "pdf")
        {
            var columnHeaders = new[] { "Date", "Guard Name", "Guard Code", "Guard Phone", "Site", "Check In", "Check Out", "Status", "Remarks" };
            var rows = items.Select(i => new[]
            {
                i.AttendanceDate.ToString("yyyy-MM-dd"),
                i.GuardName,
                i.GuardCode,
                i.GuardPhone ?? "",
                i.SiteName,
                i.CheckInTime?.ToString("HH:mm") ?? "",
                i.CheckOutTime?.ToString("HH:mm") ?? "",
                i.Status,
                i.Remarks ?? ""
            }).ToList();
            var bytes = ExportHelper.ToPdf("Attendance Report", columnHeaders, rows, header);
            return File(bytes, "application/pdf", $"Attendance_{stamp}.pdf");
        }
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };
        using var memory = new MemoryStream();
        using (var writer = new StreamWriter(memory, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, csvConfig))
        {
            await csv.WriteRecordsAsync(items, cancellationToken);
        }
        memory.Position = 0;
        return File(memory.ToArray(), "text/csv", $"Attendance_{stamp}.csv");
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
