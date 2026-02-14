using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Expense : TenantEntity
{
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string Category { get; set; } = string.Empty; // Travel, Equipment, Training, Maintenance, Office, Other
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Cheque, BankTransfer, UPI, CreditCard
    public string? VendorName { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? ReceiptPath { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? GuardId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Paid
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Site? Site { get; set; }
    public virtual SecurityGuard? Guard { get; set; }
    public virtual User? ApprovedByUser { get; set; }
}
