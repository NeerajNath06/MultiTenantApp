namespace SecurityAgencyApp.Web.Models.Api;

public class BillListResponse
{
    public List<BillItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class BillItemDto
{
    public Guid Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public Guid? SiteId { get; set; }
    public string? SiteName { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class BillDetailDto
{
    public Guid Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public Guid? SiteId { get; set; }
    public string? SiteName { get; set; }
    public Guid? ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public List<BillLineItemDto> Items { get; set; } = new();
}

public class BillLineItemDto
{
    public Guid Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CreateBillRequest
{
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public Guid? SiteId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public List<CreateBillLineItemRequest> Items { get; set; } = new();
}

public class CreateBillLineItemRequest
{
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal DiscountAmount { get; set; }
}

public class UpdateBillRequest
{
    public Guid Id { get; set; }
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
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public List<CreateBillLineItemRequest> Items { get; set; } = new();
}
