namespace SecurityAgencyApp.Web.Models.Api;

public class ComplianceSummaryResponse
{
    public int CompliantCount { get; set; }
    public int WarningCount { get; set; }
    public int NonCompliantCount { get; set; }
    public int OverallScorePercent { get; set; }
    public List<ComplianceItemDto> Items { get; set; } = new();
}

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
