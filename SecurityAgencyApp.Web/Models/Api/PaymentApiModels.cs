namespace SecurityAgencyApp.Web.Models.Api;

public class PaymentListResponse
{
    public List<PaymentItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PaymentItemDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid? BillId { get; set; }
    public string? BillNumber { get; set; }
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ReceivedDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class PaymentDetailDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid? BillId { get; set; }
    public string? BillNumber { get; set; }
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public Guid? ContractId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ChequeNumber { get; set; }
    public string? BankName { get; set; }
    public string? TransactionReference { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class CreatePaymentRequest
{
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid? BillId { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ContractId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? ChequeNumber { get; set; }
    public string? BankName { get; set; }
    public string? TransactionReference { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public DateTime? ReceivedDate { get; set; }
}

public class UpdatePaymentRequest : CreatePaymentRequest
{
    public Guid Id { get; set; }
}
