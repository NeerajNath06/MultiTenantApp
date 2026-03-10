using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Bills.Commands.DeleteBill;

public class DeleteBillCommandHandler : IRequestHandler<DeleteBillCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteBillCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteBillCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");

        var billRepo = _unitOfWork.Repository<Bill>();
        var bill = await billRepo.GetByIdAsync(request.Id, cancellationToken);
        if (bill == null || bill.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Bill not found");

        bill.IsActive = false;
        bill.ModifiedDate = DateTime.UtcNow;
        bill.Status = "Cancelled";
        await billRepo.UpdateAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Bill deleted successfully");
    }
}
