using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.API.Services;
using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.MonthlyDocuments.Commands.GenerateMonthlyDocuments;

namespace SecurityAgencyApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly MonthlyReportService _reportService;
    private readonly IMediator _mediator;

    public ReportController(MonthlyReportService reportService, IMediator mediator)
    {
        _reportService = reportService;
        _mediator = mediator;
    }

    /// <summary>Generate Bill report for the given site, year, month. format=Excel|PDF.</summary>
    [HttpGet("generate-bill")]
    public async Task<IActionResult> GenerateBill(
        [FromQuery] Guid siteId,
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string format = "Excel",
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure snapshots exist (duplicates allowed) – attendance-driven generation
            await _mediator.Send(new GenerateMonthlyDocumentsCommand { SiteId = siteId, Year = year, Month = month }, cancellationToken);
            var (content, contentType, fileName) = await _reportService.GenerateAsync(siteId, year, month, format, MonthlyReportService.ReportType.Bill, cancellationToken);
            return File(content, contentType, fileName);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ApiResponse<object?>.ErrorResponse(
                "Validation failed",
                new[] { ApiError.Create(ex.Message) }));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object?>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Generate Attendance report for the given site, year, month. format=Excel|PDF.</summary>
    [HttpGet("generate-attendance")]
    public async Task<IActionResult> GenerateAttendance(
        [FromQuery] Guid siteId,
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string format = "Excel",
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _mediator.Send(new GenerateMonthlyDocumentsCommand { SiteId = siteId, Year = year, Month = month }, cancellationToken);
            var (content, contentType, fileName) = await _reportService.GenerateAsync(siteId, year, month, format, MonthlyReportService.ReportType.Attendance, cancellationToken);
            return File(content, contentType, fileName);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ApiResponse<object?>.ErrorResponse(
                "Validation failed",
                new[] { ApiError.Create(ex.Message) }));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object?>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Generate Wages report for the given site, year, month. format=Excel|PDF.</summary>
    [HttpGet("generate-wages")]
    public async Task<IActionResult> GenerateWages(
        [FromQuery] Guid siteId,
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string format = "Excel",
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _mediator.Send(new GenerateMonthlyDocumentsCommand { SiteId = siteId, Year = year, Month = month }, cancellationToken);
            var (content, contentType, fileName) = await _reportService.GenerateAsync(siteId, year, month, format, MonthlyReportService.ReportType.Wages, cancellationToken);
            return File(content, contentType, fileName);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ApiResponse<object?>.ErrorResponse(
                "Validation failed",
                new[] { ApiError.Create(ex.Message) }));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object?>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Generate full report (Bill + Attendance + Wages) in one file. Excel = 3 sheets, PDF = 3 pages.</summary>
    [HttpGet("generate-full-report")]
    public async Task<IActionResult> GenerateFullReport(
        [FromQuery] Guid siteId,
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string format = "Excel",
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _mediator.Send(new GenerateMonthlyDocumentsCommand { SiteId = siteId, Year = year, Month = month }, cancellationToken);
            var (content, contentType, fileName) = await _reportService.GenerateAsync(siteId, year, month, format, MonthlyReportService.ReportType.Full, cancellationToken);
            return File(content, contentType, fileName);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ApiResponse<object?>.ErrorResponse(
                "Validation failed",
                new[] { ApiError.Create(ex.Message) }));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object?>.ErrorResponse(ex.Message));
        }
    }
}
