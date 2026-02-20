namespace SecurityAgencyApp.Web.Models.Api;

public class AssignmentListResponse
{
    public List<AssignmentItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? Search { get; set; }
}

public class AssignmentItemDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public string GuardName { get; set; } = string.Empty;
    public string GuardCode { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public Guid ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public string? ShiftStartTime { get; set; }
    public string? ShiftEndTime { get; set; }
    public DateTime AssignmentStartDate { get; set; }
    public DateTime? AssignmentEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

public class CreateAssignmentRequest
{
    public Guid GuardId { get; set; }
    public Guid SiteId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid? SupervisorId { get; set; }
    public DateTime AssignmentStartDate { get; set; }
    public DateTime? AssignmentEndDate { get; set; }
    public string? Remarks { get; set; }
}
