namespace SecurityAgencyApp.Model.Api;

public class VehicleLogSiteSummaryDto
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string? SiteAddress { get; set; }
    public int TotalEntries { get; set; }
    public int InsideCount { get; set; }
    public int ExitedCount { get; set; }
}
