using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Payment : TenantEntity
{
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid? BillId { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ContractId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Cheque, BankTransfer, UPI, CreditCard, DebitCard
    public string? ChequeNumber { get; set; }
    public string? BankName { get; set; }
    public string? TransactionReference { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
    public string? Notes { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Bill? Bill { get; set; }
    public virtual Client? Client { get; set; }
    public virtual Contract? Contract { get; set; }
}
