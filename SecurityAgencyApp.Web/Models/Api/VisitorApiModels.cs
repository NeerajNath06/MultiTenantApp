namespace SecurityAgencyApp.Web.Models.Api;

public class VisitorListResponse
{
    public List<VisitorItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class VisitorItemDto
{
    public Guid Id { get; set; }
    public string VisitorName { get; set; } = string.Empty;
    public string VisitorType { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? HostName { get; set; }
    public Guid SiteId { get; set; }
    public string? SiteName { get; set; }
    public Guid GuardId { get; set; }
    public string? GuardName { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public string? IdProofType { get; set; }
    public string? IdProofNumber { get; set; }
    public string? BadgeNumber { get; set; }
}

public class CreateVisitorRequest
{
    public string VisitorName { get; set; } = string.Empty;
    public string VisitorType { get; set; } = "Individual";
    public string? CompanyName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? HostName { get; set; }
    public string? HostDepartment { get; set; }
    public Guid SiteId { get; set; }
    public Guid GuardId { get; set; }
    public string? IdProofType { get; set; }
    public string? IdProofNumber { get; set; }
}
