using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Payments.Commands.UpdatePayment;

public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdatePaymentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var paymentRepo = _unitOfWork.Repository<Payment>();
        var payment = await paymentRepo.GetByIdAsync(request.Id, cancellationToken);

        if (payment == null || payment.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Payment not found");
        }

        // Check if payment number exists (excluding current payment)
        var existing = await paymentRepo.FirstOrDefaultAsync(
            p => p.PaymentNumber == request.PaymentNumber && 
                 p.Id != request.Id && 
                 p.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<bool>.ErrorResponse("Payment number already exists");
        }

        payment.PaymentNumber = request.PaymentNumber;
        payment.BillId = request.BillId;
        payment.ClientId = request.ClientId;
        payment.ContractId = request.ContractId;
        payment.PaymentDate = request.PaymentDate;
        payment.Amount = request.Amount;
        payment.PaymentMethod = request.PaymentMethod;
        payment.ChequeNumber = request.ChequeNumber;
        payment.BankName = request.BankName;
        payment.TransactionReference = request.TransactionReference;
        payment.Status = request.Status;
        payment.Notes = request.Notes;
        payment.ReceivedDate = request.ReceivedDate;
        payment.ModifiedDate = DateTime.UtcNow;

        await paymentRepo.UpdateAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Payment updated successfully");
    }
}
