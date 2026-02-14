namespace SecurityAgencyApp.Web.Models.Api;

public class IncidentListResponse
{
    public List<IncidentItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? Search { get; set; }
}

public class IncidentItemDto
{
    public Guid Id { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public Guid? GuardId { get; set; }
    public string? GuardName { get; set; }
    public DateTime IncidentDate { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateIncidentRequest
{
    public Guid SiteId { get; set; }
    public Guid? GuardId { get; set; }
    public DateTime IncidentDate { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Description { get; set; } = string.Empty;
    public string? ActionTaken { get; set; }
}

public class UpdateIncidentRequest
{
    public Guid Id { get; set; }
    public string? ActionTaken { get; set; }
    public string Status { get; set; } = string.Empty; // Open, InProgress, Resolved, Closed
}
