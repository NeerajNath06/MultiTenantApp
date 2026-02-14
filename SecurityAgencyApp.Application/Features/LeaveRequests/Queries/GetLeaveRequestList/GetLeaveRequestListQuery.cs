using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.LeaveRequests.Queries.GetLeaveRequestList;

public class GetLeaveRequestListQuery : IRequest<ApiResponse<LeaveRequestListResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public Guid? GuardId { get; set; }
    /// <summary>When set (e.g. supervisor), only leave requests for guards under this supervisor are returned.</summary>
    public Guid? SupervisorId { get; set; }
    public string? Status { get; set; }
    public string? LeaveType { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}

public class LeaveRequestListResponseDto
{
    public List<LeaveRequestDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class LeaveRequestDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public string GuardName { get; set; } = string.Empty;
    public string GuardCode { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ApprovedDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
