using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Payments.Queries.GetPaymentById;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, ApiResponse<PaymentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetPaymentByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<PaymentDto>.ErrorResponse("Tenant context not found");
        }

        var paymentRepo = _unitOfWork.Repository<Payment>();
        var payment = await paymentRepo.GetByIdAsync(request.Id, cancellationToken);

        if (payment == null || payment.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<PaymentDto>.ErrorResponse("Payment not found");
        }

        Bill? bill = null;
        if (payment.BillId.HasValue)
        {
            bill = await _unitOfWork.Repository<Bill>().GetByIdAsync(payment.BillId.Value, cancellationToken);
        }

        Client? client = null;
        if (payment.ClientId.HasValue)
        {
            client = await _unitOfWork.Repository<Client>().GetByIdAsync(payment.ClientId.Value, cancellationToken);
        }

        var paymentDto = new PaymentDto
        {
            Id = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            BillId = payment.BillId,
            BillNumber = bill?.BillNumber,
            ClientId = payment.ClientId,
            ClientName = client?.CompanyName,
            ContractId = payment.ContractId,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            ChequeNumber = payment.ChequeNumber,
            BankName = payment.BankName,
            TransactionReference = payment.TransactionReference,
            Status = payment.Status,
            Notes = payment.Notes,
            ReceivedDate = payment.ReceivedDate,
            IsActive = payment.IsActive,
            CreatedDate = payment.CreatedDate,
            ModifiedDate = payment.ModifiedDate
        };

        return ApiResponse<PaymentDto>.SuccessResponse(paymentDto, "Payment retrieved successfully");
    }
}
