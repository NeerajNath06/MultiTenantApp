namespace SecurityAgencyApp.Web.Models.Api;

public class WageListResponse
{
    public List<WageItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class WageItemDto
{
    public Guid Id { get; set; }
    public string WageSheetNumber { get; set; } = string.Empty;
    public DateTime WagePeriodStart { get; set; }
    public DateTime WagePeriodEnd { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalWages { get; set; }
    public decimal NetAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

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

public class WageDetailItemRequest
{
    public Guid GuardId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? ShiftId { get; set; }
    public int DaysWorked { get; set; }
    public int HoursWorked { get; set; }
    public decimal BasicRate { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeRate { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public string? Remarks { get; set; }
}
