namespace SecurityAgencyApp.Model.Api;

public class MonthlySiteSummaryItemDto
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public int PresentDaysTotal { get; set; }
    public decimal? LatestBillTotal { get; set; }
    public decimal? LatestWageTotal { get; set; }
}
