using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Expenses.Commands.CreateExpense;

public class CreateExpenseCommand : IRequest<ApiResponse<Guid>>
{
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
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
