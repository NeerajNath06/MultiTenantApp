using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommand : IRequest<ApiResponse<Guid>>
{
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid? BillId { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ContractId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? ChequeNumber { get; set; }
    public string? BankName { get; set; }
    public string? TransactionReference { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public DateTime? ReceivedDate { get; set; }
}
