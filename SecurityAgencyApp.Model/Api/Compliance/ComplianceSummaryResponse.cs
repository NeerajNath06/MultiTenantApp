namespace SecurityAgencyApp.Model.Api;

public class ComplianceSummaryResponse
{
    public int CompliantCount { get; set; }
    public int WarningCount { get; set; }
    public int NonCompliantCount { get; set; }
    public int OverallScorePercent { get; set; }
    public List<ComplianceItemDto> Items { get; set; } = new();
}
