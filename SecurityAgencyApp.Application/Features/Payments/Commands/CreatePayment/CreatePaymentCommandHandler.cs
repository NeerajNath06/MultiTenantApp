using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreatePaymentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        // Validate bill if provided
        if (request.BillId.HasValue)
        {
            var bill = await _unitOfWork.Repository<Bill>().GetByIdAsync(request.BillId.Value, cancellationToken);
            if (bill == null || bill.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<Guid>.ErrorResponse("Invalid bill");
            }
        }

        // Validate client if provided
        if (request.ClientId.HasValue)
        {
            var client = await _unitOfWork.Repository<Client>().GetByIdAsync(request.ClientId.Value, cancellationToken);
            if (client == null || client.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<Guid>.ErrorResponse("Invalid client");
            }
        }

        // Validate contract if provided
        if (request.ContractId.HasValue)
        {
            var contract = await _unitOfWork.Repository<Contract>().GetByIdAsync(request.ContractId.Value, cancellationToken);
            if (contract == null || contract.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<Guid>.ErrorResponse("Invalid contract");
            }
        }

        // Check if payment number exists
        var paymentRepo = _unitOfWork.Repository<Payment>();
        var existing = await paymentRepo.FirstOrDefaultAsync(
            p => p.PaymentNumber == request.PaymentNumber && p.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<Guid>.ErrorResponse("Payment number already exists");
        }

        var payment = new Payment
        {
            TenantId = _tenantContext.TenantId.Value,
            PaymentNumber = request.PaymentNumber,
            BillId = request.BillId,
            ClientId = request.ClientId,
            ContractId = request.ContractId,
            PaymentDate = request.PaymentDate,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            ChequeNumber = request.ChequeNumber,
            BankName = request.BankName,
            TransactionReference = request.TransactionReference,
            Status = request.Status,
            Notes = request.Notes,
            ReceivedDate = request.ReceivedDate,
            IsActive = true
        };

        await paymentRepo.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(payment.Id, "Payment created successfully");
    }
}
