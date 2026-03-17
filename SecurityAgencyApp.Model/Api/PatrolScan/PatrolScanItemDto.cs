namespace SecurityAgencyApp.Model.Api;

public class PatrolScanItemDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public Guid SiteId { get; set; }
    public string? SiteName { get; set; }
    public DateTime ScannedAt { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? CheckpointCode { get; set; }
    public string Status { get; set; } = string.Empty;
}
