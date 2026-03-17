namespace SecurityAgencyApp.Model.Api;

public class ContractSiteItemDto
{
    public Guid SiteId { get; set; }
    public string? SiteName { get; set; }
    public int RequiredGuards { get; set; }
    public string? ShiftDetails { get; set; }
}
