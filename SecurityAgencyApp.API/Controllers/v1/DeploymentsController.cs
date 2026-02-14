using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.GuardAssignments.Queries.GetAssignmentList;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.API.Controllers.v1;

/// <summary>
/// Deployments API for mobile app (check-in flow). Returns guard assignments in deployment shape.
/// GET /api/v1/Deployments?guardId=... returns list so mobile can get siteId/shiftId for check-in.
/// GET /api/v1/Deployments/roster returns roster (per-day deployments) for supervisor dashboard.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class DeploymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public DeploymentsController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get deployments (assignments) for a guard. Used by mobile for check-in.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DeploymentDto>>>> GetDeployments(
        [FromQuery] Guid? guardId = null,
        [FromQuery] Guid? siteId = null,
        [FromQuery] string? dateFrom = null,
        [FromQuery] string? dateTo = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetAssignmentListQuery
        {
            GuardId = guardId,
            SiteId = siteId,
            IncludeInactive = false,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = "date",
            SortDirection = "desc",
            DateFrom = !string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out var df) ? df : null,
            DateTo = !string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out var dt) ? dt : null
        };
        var result = await _mediator.Send(query);
        if (!result.Success || result.Data == null)
            return Ok(ApiResponse<List<DeploymentDto>>.SuccessResponse(new List<DeploymentDto>(), result.Message ?? "No deployments"));

        var items = result.Data.Items;
        if (items.Count == 0)
            return Ok(ApiResponse<List<DeploymentDto>>.SuccessResponse(new List<DeploymentDto>(), "No deployments"));

        var shiftIds = items.Select(a => a.ShiftId).Distinct().ToList();
        var shifts = await _unitOfWork.Repository<Shift>().FindAsync(s => shiftIds.Contains(s.Id), CancellationToken.None);
        var shiftMap = shifts.ToDictionary(s => s.Id, s => s);

        var list = items.Select(a =>
        {
            var shift = shiftMap.GetValueOrDefault(a.ShiftId);
            return new DeploymentDto
            {
                Id = a.Id,
                GuardId = a.GuardId,
                GuardName = a.GuardName,
                SiteId = a.SiteId,
                SiteName = a.SiteName,
                ShiftId = a.ShiftId,
                ShiftName = a.ShiftName,
                DeploymentDate = a.AssignmentStartDate.ToString("yyyy-MM-dd"),
                StartTime = shift != null ? shift.StartTime.ToString(@"hh\:mm") : "00:00",
                EndTime = shift != null ? shift.EndTime.ToString(@"hh\:mm") : "23:59",
                Status = a.Status,
                SupervisorId = a.SupervisorId,
                SupervisorName = a.SupervisorName
            };
        }).ToList();

        return Ok(ApiResponse<List<DeploymentDto>>.SuccessResponse(list, "Deployments retrieved"));
    }

    /// <summary>
    /// Get roster: deployments expanded per day for the date range. Used by supervisor roster management.
    /// </summary>
    [HttpGet("roster")]
    public async Task<ActionResult<ApiResponse<RosterDto>>> GetRoster(
        [FromQuery] string dateFrom,
        [FromQuery] string dateTo,
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? supervisorId = null)
    {
        if (string.IsNullOrEmpty(dateFrom) || string.IsNullOrEmpty(dateTo) ||
            !DateTime.TryParse(dateFrom, out var fromDate) || !DateTime.TryParse(dateTo, out var toDate))
        {
            return BadRequest(ApiResponse<RosterDto>.ErrorResponse("dateFrom and dateTo are required (yyyy-MM-dd)."));
        }
        if (fromDate > toDate)
        {
            return BadRequest(ApiResponse<RosterDto>.ErrorResponse("dateFrom must be less than or equal to dateTo."));
        }

        var query = new GetAssignmentListQuery
        {
            SiteId = siteId,
            SupervisorId = supervisorId,
            DateFrom = fromDate,
            DateTo = toDate,
            IncludeInactive = false,
            PageNumber = 1,
            PageSize = 5000,
            SortBy = "date",
            SortDirection = "asc"
        };
        var result = await _mediator.Send(query);
        if (!result.Success || result.Data == null)
            return Ok(ApiResponse<RosterDto>.SuccessResponse(new RosterDto { DateFrom = dateFrom, DateTo = dateTo, Deployments = new List<DeploymentDto>() }, result.Message ?? "No roster data"));

        var items = result.Data.Items;
        var shiftIds = items.Select(a => a.ShiftId).Distinct().ToList();
        var shifts = await _unitOfWork.Repository<Shift>().FindAsync(s => shiftIds.Contains(s.Id), CancellationToken.None);
        var shiftMap = shifts.ToDictionary(s => s.Id, s => s);

        var deployments = new List<DeploymentDto>();
        for (var d = fromDate.Date; d <= toDate.Date; d = d.AddDays(1))
        {
            foreach (var a in items)
            {
                if (d < a.AssignmentStartDate.Date) continue;
                if (a.AssignmentEndDate.HasValue && d > a.AssignmentEndDate.Value.Date) continue;
                var shift = shiftMap.GetValueOrDefault(a.ShiftId);
                deployments.Add(new DeploymentDto
                {
                    Id = a.Id,
                    GuardId = a.GuardId,
                    GuardName = a.GuardName,
                    SiteId = a.SiteId,
                    SiteName = a.SiteName,
                    ShiftId = a.ShiftId,
                    ShiftName = a.ShiftName,
                    DeploymentDate = d.ToString("yyyy-MM-dd"),
                    StartTime = shift != null ? shift.StartTime.ToString(@"hh\:mm") : "00:00",
                    EndTime = shift != null ? shift.EndTime.ToString(@"hh\:mm") : "23:59",
                    Status = a.Status,
                    SupervisorId = a.SupervisorId,
                    SupervisorName = a.SupervisorName
                });
            }
        }

        var roster = new RosterDto
        {
            DateFrom = fromDate.ToString("yyyy-MM-dd"),
            DateTo = toDate.ToString("yyyy-MM-dd"),
            Deployments = deployments
        };
        return Ok(ApiResponse<RosterDto>.SuccessResponse(roster, "Roster retrieved"));
    }
}

public class RosterDto
{
    public string DateFrom { get; set; } = string.Empty;
    public string DateTo { get; set; } = string.Empty;
    public List<DeploymentDto> Deployments { get; set; } = new();
}

/// <summary>Mobile expects this shape for deployment (check-in).</summary>
public class DeploymentDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public string? GuardName { get; set; }
    public Guid SiteId { get; set; }
    public string? SiteName { get; set; }
    public Guid ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public string DeploymentDate { get; set; } = string.Empty;
    public string StartTime { get; set; } = "00:00";
    public string EndTime { get; set; } = "23:59";
    public string Status { get; set; } = string.Empty;
    public Guid? SupervisorId { get; set; }
    public string? SupervisorName { get; set; }
}
