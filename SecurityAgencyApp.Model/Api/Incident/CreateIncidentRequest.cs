namespace SecurityAgencyApp.Model.Api;

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
