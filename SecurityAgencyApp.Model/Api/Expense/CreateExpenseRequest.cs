namespace SecurityAgencyApp.Model.Api;

public class CreateExpenseRequest
{
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? VendorName { get; set; }
    public string? ReceiptNumber { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? GuardId { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
}
