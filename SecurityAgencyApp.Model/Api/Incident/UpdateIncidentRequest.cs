namespace SecurityAgencyApp.Model.Api;

public class UpdateIncidentRequest
{
    public Guid Id { get; set; }
    public string? ActionTaken { get; set; }
    public string Status { get; set; } = string.Empty; // Open, InProgress, Resolved, Closed
}
