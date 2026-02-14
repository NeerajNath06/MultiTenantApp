using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Bills.Commands.CreateBill;

public class CreateBillCommandHandler : IRequestHandler<CreateBillCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateBillCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateBillCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return ApiResponse<Guid>.ErrorResponse("At least one bill item is required");
        }

        // Validate site if provided
        if (request.SiteId.HasValue)
        {
            var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId.Value, cancellationToken);
            if (site == null || site.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<Guid>.ErrorResponse("Invalid site");
            }
        }

        // Calculate totals
        decimal subTotal = 0;
        decimal totalTax = 0;
        decimal totalDiscount = 0;

        foreach (var item in request.Items)
        {
            var itemSubTotal = item.Quantity * item.UnitPrice;
            var itemDiscount = item.DiscountAmount;
            var itemAfterDiscount = itemSubTotal - itemDiscount;
            var itemTax = itemAfterDiscount * (item.TaxRate / 100);
            
            subTotal += itemSubTotal;
            totalDiscount += itemDiscount;
            totalTax += itemTax;
        }

        var totalAmount = subTotal - totalDiscount + totalTax + request.TaxAmount - request.DiscountAmount;

        var bill = new Bill
        {
            TenantId = _tenantContext.TenantId.Value,
            BillNumber = request.BillNumber,
            BillDate = request.BillDate,
            SiteId = request.SiteId,
            ClientName = request.ClientName,
            Description = request.Description,
            SubTotal = subTotal,
            TaxAmount = totalTax + request.TaxAmount,
            DiscountAmount = totalDiscount + request.DiscountAmount,
            TotalAmount = totalAmount,
            PaymentTerms = request.PaymentTerms,
            DueDate = request.DueDate,
            Status = request.Status,
            Notes = request.Notes,
            IsActive = true
        };

        await _unitOfWork.Repository<Bill>().AddAsync(bill, cancellationToken);

        // Add bill items
        foreach (var itemDto in request.Items)
        {
            var itemSubTotal = itemDto.Quantity * itemDto.UnitPrice;
            var itemAfterDiscount = itemSubTotal - itemDto.DiscountAmount;
            var itemTax = itemAfterDiscount * (itemDto.TaxRate / 100);
            var itemTotal = itemAfterDiscount + itemTax;

            var billItem = new BillItem
            {
                BillId = bill.Id,
                ItemName = itemDto.ItemName,
                Description = itemDto.Description,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                TaxRate = itemDto.TaxRate,
                TaxAmount = itemTax,
                DiscountAmount = itemDto.DiscountAmount,
                TotalAmount = itemTotal
            };

            await _unitOfWork.Repository<BillItem>().AddAsync(billItem, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(bill.Id, "Bill created successfully");
    }
}
