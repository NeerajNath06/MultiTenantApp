namespace SecurityAgencyApp.Model.Api;

public class ComplianceItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? DueDate { get; set; }
    public string Details { get; set; } = string.Empty;
    public int AffectedCount { get; set; }
}
