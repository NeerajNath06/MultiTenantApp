namespace SecurityAgencyApp.Web.Models.Api;

public class ExpenseListResponse
{
    public List<ExpenseItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ExpenseItemDto
{
    public Guid Id { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? VendorName { get; set; }
    public Guid? SiteId { get; set; }
    public string? SiteName { get; set; }
    public Guid? GuardId { get; set; }
    public string? GuardName { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

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
