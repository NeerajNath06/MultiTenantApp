using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Bills.Commands.UpdateBill;

public class UpdateBillCommandHandler : IRequestHandler<UpdateBillCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateBillCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateBillCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var billRepo = _unitOfWork.Repository<Bill>();
        var bill = await billRepo.GetByIdAsync(request.Id, cancellationToken);

        if (bill == null || bill.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Bill not found");
        }

        // Check if bill number exists (excluding current bill)
        var existing = await billRepo.FirstOrDefaultAsync(
            b => b.BillNumber == request.BillNumber && 
                 b.Id != request.Id && 
                 b.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<bool>.ErrorResponse("Bill number already exists");
        }

        // Update bill properties
        bill.BillNumber = request.BillNumber;
        bill.BillDate = request.BillDate;
        bill.SiteId = request.SiteId;
        bill.ClientId = request.ClientId;
        bill.ClientName = request.ClientName;
        bill.Description = request.Description;
        bill.PaymentTerms = request.PaymentTerms;
        bill.DueDate = request.DueDate;
        bill.Status = request.Status;
        bill.Notes = request.Notes;
        bill.ModifiedDate = DateTime.UtcNow;
        bill.ModifiedBy = _currentUserService.UserId;

        // Delete existing items
        var existingItems = await _unitOfWork.Repository<BillItem>().FindAsync(
            bi => bi.BillId == bill.Id, cancellationToken);
        var itemRepo = _unitOfWork.Repository<BillItem>();
        foreach (var item in existingItems)
        {
            await itemRepo.DeleteAsync(item, cancellationToken);
        }

        // Add new items
        decimal subTotal = 0;
        decimal totalTax = 0;
        decimal totalDiscount = 0;

        foreach (var itemDto in request.Items)
        {
            var itemAmount = itemDto.Quantity * itemDto.UnitPrice;
            var itemDiscount = itemDto.DiscountAmount;
            var itemAfterDiscount = itemAmount - itemDiscount;
            var itemTax = itemAfterDiscount * (itemDto.TaxRate / 100);
            var itemTotal = itemAfterDiscount + itemTax;

            subTotal += itemAmount;
            totalDiscount += itemDiscount;
            totalTax += itemTax;

            var billItem = new BillItem
            {
                BillId = bill.Id,
                ItemName = itemDto.ItemName,
                Description = itemDto.Description,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                TaxRate = itemDto.TaxRate,
                TaxAmount = itemTax,
                DiscountAmount = itemDiscount,
                TotalAmount = itemTotal
            };

            await itemRepo.AddAsync(billItem, cancellationToken);
        }

        bill.SubTotal = subTotal;
        bill.DiscountAmount = totalDiscount;
        bill.TaxAmount = totalTax;
        bill.TotalAmount = subTotal - totalDiscount + totalTax;

        await billRepo.UpdateAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Bill updated successfully");
    }
}
