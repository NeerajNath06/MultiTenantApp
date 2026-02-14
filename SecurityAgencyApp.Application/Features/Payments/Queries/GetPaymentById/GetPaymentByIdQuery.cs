using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Payments.Queries.GetPaymentById;

public class GetPaymentByIdQuery : IRequest<ApiResponse<PaymentDto>>
{
    public Guid Id { get; set; }
}

public class PaymentDto
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
