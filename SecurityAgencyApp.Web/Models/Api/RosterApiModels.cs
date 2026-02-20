namespace SecurityAgencyApp.Web.Models.Api;

/// <summary>Roster response from GET api/v1/Deployments/roster.</summary>
public class RosterResponse
{
    public string DateFrom { get; set; } = string.Empty;
    public string DateTo { get; set; } = string.Empty;
    public List<RosterDeploymentDto> Deployments { get; set; } = new();
}

public class RosterDeploymentDto
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
