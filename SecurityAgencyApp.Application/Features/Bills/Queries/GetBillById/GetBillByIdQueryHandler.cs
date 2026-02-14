using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Bills.Queries.GetBillById;

public class GetBillByIdQueryHandler : IRequestHandler<GetBillByIdQuery, ApiResponse<BillDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetBillByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<BillDto>> Handle(GetBillByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<BillDto>.ErrorResponse("Tenant context not found");
        }

        var billRepo = _unitOfWork.Repository<Bill>();
        var bill = await billRepo.GetByIdAsync(request.Id, cancellationToken);

        if (bill == null || bill.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<BillDto>.ErrorResponse("Bill not found");
        }

        Site? site = null;
        if (bill.SiteId.HasValue)
        {
            site = await _unitOfWork.Repository<Site>().GetByIdAsync(bill.SiteId.Value, cancellationToken);
        }

        Client? client = null;
        if (bill.ClientId.HasValue)
        {
            client = await _unitOfWork.Repository<Client>().GetByIdAsync(bill.ClientId.Value, cancellationToken);
        }

        var billItems = await _unitOfWork.Repository<BillItem>().FindAsync(
            bi => bi.BillId == bill.Id, cancellationToken);

        var billDto = new BillDto
        {
            Id = bill.Id,
            BillNumber = bill.BillNumber,
            BillDate = bill.BillDate,
            SiteId = bill.SiteId,
            SiteName = site?.SiteName,
            ClientId = bill.ClientId,
            ClientName = bill.ClientName,
            Description = bill.Description,
            SubTotal = bill.SubTotal,
            TaxAmount = bill.TaxAmount,
            DiscountAmount = bill.DiscountAmount,
            TotalAmount = bill.TotalAmount,
            PaymentTerms = bill.PaymentTerms,
            DueDate = bill.DueDate,
            Status = bill.Status,
            Notes = bill.Notes,
            IsActive = bill.IsActive,
            CreatedDate = bill.CreatedDate,
            ModifiedDate = bill.ModifiedDate,
            Items = billItems.Select(bi => new BillItemDto
            {
                Id = bi.Id,
                ItemName = bi.ItemName,
                Description = bi.Description,
                Quantity = bi.Quantity,
                UnitPrice = bi.UnitPrice,
                TaxRate = bi.TaxRate,
                TaxAmount = bi.TaxAmount,
                DiscountAmount = bi.DiscountAmount,
                TotalAmount = bi.TotalAmount
            }).ToList()
        };

        return ApiResponse<BillDto>.SuccessResponse(billDto, "Bill retrieved successfully");
    }
}
