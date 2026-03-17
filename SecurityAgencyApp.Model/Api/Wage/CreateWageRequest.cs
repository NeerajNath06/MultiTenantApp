namespace SecurityAgencyApp.Model.Api;

public class CreateWageRequest
{
    public string WageSheetNumber { get; set; } = string.Empty;
    public DateTime WagePeriodStart { get; set; }
    public DateTime WagePeriodEnd { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public string? Notes { get; set; }
    public List<WageDetailItemRequest> WageDetails { get; set; } = new();
}
