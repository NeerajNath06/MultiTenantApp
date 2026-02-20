using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.GuardAssignments.Queries.GetAssignmentList;

public class GetAssignmentListQuery : IRequest<ApiResponse<AssignmentListResponseDto>>
{
    public Guid? GuardId { get; set; }
    public Guid? SiteId { get; set; }
    /// <summary>Filter assignments by supervisor (User ID). Only assignments where this user is the supervisor are returned.</summary>
    public Guid? SupervisorId { get; set; }
    /// <summary>Filter assignments that overlap this date (AssignmentStartDate &lt;= dateTo and (AssignmentEndDate == null or &gt;= dateFrom)).</summary>
    public DateTime? DateFrom { get; set; }
    /// <summary>Filter assignments that overlap this date.</summary>
    public DateTime? DateTo { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
}

public class AssignmentListResponseDto
{
    public List<AssignmentDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? Search { get; set; }
}

public class AssignmentDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public string GuardName { get; set; } = string.Empty;
    public string GuardCode { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public Guid ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    /// <summary>Shift start time (e.g. "08:00") for roster/display.</summary>
    public string? ShiftStartTime { get; set; }
    /// <summary>Shift end time (e.g. "16:00") for roster/display.</summary>
    public string? ShiftEndTime { get; set; }
    public DateTime AssignmentStartDate { get; set; }
    public DateTime? AssignmentEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public Guid? SupervisorId { get; set; }
    public string? SupervisorName { get; set; }
}
