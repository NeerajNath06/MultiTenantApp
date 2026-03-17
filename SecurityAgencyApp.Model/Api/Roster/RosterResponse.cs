namespace SecurityAgencyApp.Model.Api;

public class RosterResponse
{
    public string DateFrom { get; set; } = string.Empty;
    public string DateTo { get; set; } = string.Empty;
    public List<RosterDeploymentDto> Deployments { get; set; } = new();
}
