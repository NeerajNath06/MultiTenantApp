namespace SecurityAgencyApp.Model.Api;

public class MonthlySiteSummaryResponseDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<MonthlySiteSummaryItemDto> Items { get; set; } = new();
}
