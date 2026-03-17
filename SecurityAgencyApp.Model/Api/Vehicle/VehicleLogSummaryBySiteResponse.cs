namespace SecurityAgencyApp.Model.Api;

public class VehicleLogSummaryBySiteResponse
{
    public List<VehicleLogSiteSummaryDto> Sites { get; set; } = new();
}
