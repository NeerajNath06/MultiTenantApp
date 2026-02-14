using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Bills.Commands.CreateBill;

public class CreateBillCommand : IRequest<ApiResponse<Guid>>
{
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; } = DateTime.UtcNow;
    public Guid? SiteId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public List<BillItemDto> Items { get; set; } = new();
}

public class BillItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal DiscountAmount { get; set; }
}
