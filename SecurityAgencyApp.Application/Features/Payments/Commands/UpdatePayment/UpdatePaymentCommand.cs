using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Payments.Commands.UpdatePayment;

public class UpdatePaymentCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
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
