using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Bills.Queries.GetBillList;

public class GetBillListQueryHandler : IRequestHandler<GetBillListQuery, ApiResponse<BillListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetBillListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<BillListResponseDto>> Handle(GetBillListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<BillListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var billRepo = _unitOfWork.Repository<Bill>();
        var query = billRepo.GetQueryable()
            .Where(b => b.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || b.IsActive));

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(b =>
                b.BillNumber.ToLower().Contains(search) ||
                b.ClientName.ToLower().Contains(search) ||
                (b.Description != null && b.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(b => b.Status == request.Status);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(b => b.BillDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(b => b.BillDate <= request.EndDate.Value);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "billnumber" => request.SortDirection == "asc"
                ? query.OrderBy(b => b.BillNumber)
                : query.OrderByDescending(b => b.BillNumber),
            "billdate" => request.SortDirection == "asc"
                ? query.OrderBy(b => b.BillDate)
                : query.OrderByDescending(b => b.BillDate),
            "totalamount" => request.SortDirection == "asc"
                ? query.OrderBy(b => b.TotalAmount)
                : query.OrderByDescending(b => b.TotalAmount),
            _ => query.OrderByDescending(b => b.BillDate)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var bills = await billRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        // Get site names
        var siteIds = bills.Where(b => b.SiteId.HasValue).Select(b => b.SiteId!.Value).Distinct().ToList();
        var sites = siteIds.Any()
            ? await _unitOfWork.Repository<Site>().FindAsync(s => siteIds.Contains(s.Id), cancellationToken)
            : new List<Site>();

        var billDtos = bills.Select(b => new BillDto
        {
            Id = b.Id,
            BillNumber = b.BillNumber,
            BillDate = b.BillDate,
            SiteId = b.SiteId,
            SiteName = sites.FirstOrDefault(s => s.Id == b.SiteId)?.SiteName,
            ClientName = b.ClientName,
            TotalAmount = b.TotalAmount,
            Status = b.Status,
            DueDate = b.DueDate,
            IsActive = b.IsActive,
            CreatedDate = b.CreatedDate
        }).ToList();

        var response = new BillListResponseDto
        {
            Items = billDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<BillListResponseDto>.SuccessResponse(response, "Bills retrieved successfully");
    }
}
