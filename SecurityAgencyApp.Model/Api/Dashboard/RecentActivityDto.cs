namespace SecurityAgencyApp.Model.Api;

public class RecentActivityDto
{
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; }
    public string UserName { get; set; } = string.Empty;
}
