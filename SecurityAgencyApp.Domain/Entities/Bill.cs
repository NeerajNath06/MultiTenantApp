using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Bill : TenantEntity
{
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Overdue, Cancelled
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Site? Site { get; set; }
    public virtual Client? Client { get; set; }
    public virtual ICollection<BillItem> Items { get; set; } = new List<BillItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class BillItem : BaseEntity
{
    public Guid BillId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Navigation properties
    public virtual Bill Bill { get; set; } = null!;
}
