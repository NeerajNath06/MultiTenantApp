using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Incidents.Commands.CreateIncident;
using SecurityAgencyApp.Application.Features.Incidents.Commands.UpdateIncident;
using SecurityAgencyApp.Application.Features.Incidents.Queries.GetIncidentList;
using SecurityAgencyApp.API.Services;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public IncidentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Export incidents as CSV, Excel (.xlsx), or PDF. Same filters as GET list. format=csv|xlsx|pdf.</summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportIncidents(
        [FromQuery] string format = "csv",
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? guardId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? severity = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var fmt = format?.Trim().ToLowerInvariant() ?? "csv";
        if (fmt != "csv" && fmt != "xlsx" && fmt != "pdf")
            return BadRequest("Supported format: csv, xlsx, pdf");

        var query = new GetIncidentListQuery
        {
            SiteId = siteId,
            GuardId = guardId,
            Status = status,
            Severity = severity,
            StartDate = startDate,
            EndDate = endDate,
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

        if (fmt == "xlsx")
        {
            var bytes = ExportHelper.ToExcel("Incidents", "Incident Reports", items);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Incidents_{stamp}.xlsx");
        }
        if (fmt == "pdf")
        {
            var headers = new[] { "Incident #", "Date", "Site", "Guard", "Type", "Severity", "Status", "Description" };
            var rows = items.Select(i => new[]
            {
                i.IncidentNumber,
                i.IncidentDate.ToString("yyyy-MM-dd HH:mm"),
                i.SiteName,
                i.GuardName ?? "",
                i.IncidentType,
                i.Severity,
                i.Status,
                i.Description.Length > 200 ? i.Description[..200] + "…" : i.Description
            }).ToList();
            var bytes = ExportHelper.ToPdf("Incident Reports", headers, rows);
            return File(bytes, "application/pdf", $"Incidents_{stamp}.pdf");
        }
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };
        using var memory = new MemoryStream();
        using (var writer = new StreamWriter(memory, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, csvConfig))
        {
            await csv.WriteRecordsAsync(items, cancellationToken);
        }
        memory.Position = 0;
        return File(memory.ToArray(), "text/csv", $"Incidents_{stamp}.csv");
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IncidentListResponseDto>>> GetIncidents(
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? guardId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? severity = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetIncidentListQuery
        {
            SiteId = siteId,
            GuardId = guardId,
            Status = status,
            Severity = severity,
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateIncident([FromBody] CreateIncidentCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return CreatedAtAction(nameof(GetIncidents), new { id = result.Data }, result);
        return BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateIncident(Guid id, [FromBody] UpdateIncidentCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }
}
