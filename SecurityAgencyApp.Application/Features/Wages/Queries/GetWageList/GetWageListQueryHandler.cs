using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Wages.Queries.GetWageList;

public class GetWageListQueryHandler : IRequestHandler<GetWageListQuery, ApiResponse<WageListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetWageListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<WageListResponseDto>> Handle(GetWageListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<WageListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var wageRepo = _unitOfWork.Repository<Wage>();
        var query = wageRepo.GetQueryable()
            .Where(w => w.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || w.IsActive));

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(w =>
                w.WageSheetNumber.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(w => w.Status == request.Status);
        }

        if (request.PeriodStart.HasValue)
        {
            query = query.Where(w => w.WagePeriodStart >= request.PeriodStart.Value);
        }

        if (request.PeriodEnd.HasValue)
        {
            query = query.Where(w => w.WagePeriodEnd <= request.PeriodEnd.Value);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "wagesheetnumber" => request.SortDirection == "asc"
                ? query.OrderBy(w => w.WageSheetNumber)
                : query.OrderByDescending(w => w.WageSheetNumber),
            "paymentdate" => request.SortDirection == "asc"
                ? query.OrderBy(w => w.PaymentDate)
                : query.OrderByDescending(w => w.PaymentDate),
            "netamount" => request.SortDirection == "asc"
                ? query.OrderBy(w => w.NetAmount)
                : query.OrderByDescending(w => w.NetAmount),
            _ => query.OrderByDescending(w => w.PaymentDate)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var wages = await wageRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var wageDtos = wages.Select(w => new WageDto
        {
            Id = w.Id,
            WageSheetNumber = w.WageSheetNumber,
            WagePeriodStart = w.WagePeriodStart,
            WagePeriodEnd = w.WagePeriodEnd,
            PaymentDate = w.PaymentDate,
            Status = w.Status,
            TotalWages = w.TotalWages,
            NetAmount = w.NetAmount,
            IsActive = w.IsActive,
            CreatedDate = w.CreatedDate
        }).ToList();

        var response = new WageListResponseDto
        {
            Items = wageDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<WageListResponseDto>.SuccessResponse(response, "Wages retrieved successfully");
    }
}
