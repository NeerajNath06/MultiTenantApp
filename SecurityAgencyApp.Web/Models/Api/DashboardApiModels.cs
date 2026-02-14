namespace SecurityAgencyApp.Web.Models.Api;

public class DashboardDataDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalRoles { get; set; }
    public int TotalSecurityGuards { get; set; }
    public int ActiveGuards { get; set; }
    public int TotalMenus { get; set; }
    public int TotalFormSubmissions { get; set; }
    public int PendingFormSubmissions { get; set; }
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; }
    public string UserName { get; set; } = string.Empty;
}
