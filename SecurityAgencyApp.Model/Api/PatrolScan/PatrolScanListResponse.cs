namespace SecurityAgencyApp.Model.Api;

public class PatrolScanListResponse
{
    public List<PatrolScanItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
