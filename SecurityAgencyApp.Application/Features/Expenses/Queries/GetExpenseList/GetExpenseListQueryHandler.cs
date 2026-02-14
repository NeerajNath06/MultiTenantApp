using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Expenses.Queries.GetExpenseList;

public class GetExpenseListQueryHandler : IRequestHandler<GetExpenseListQuery, ApiResponse<ExpenseListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetExpenseListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<ExpenseListResponseDto>> Handle(GetExpenseListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<ExpenseListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var expenseRepo = _unitOfWork.Repository<Expense>();
        var query = expenseRepo.GetQueryable()
            .Where(e => e.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || e.IsActive));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(e =>
                e.ExpenseNumber.ToLower().Contains(search) ||
                e.Description.ToLower().Contains(search) ||
                (e.VendorName != null && e.VendorName.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(e => e.Category == request.Category);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(e => e.Status == request.Status);
        }

        if (request.SiteId.HasValue)
        {
            query = query.Where(e => e.SiteId == request.SiteId.Value);
        }

        if (request.GuardId.HasValue)
        {
            query = query.Where(e => e.GuardId == request.GuardId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= request.EndDate.Value);
        }

        query = request.SortBy?.ToLower() switch
        {
            "expensedate" => request.SortDirection == "asc"
                ? query.OrderBy(e => e.ExpenseDate)
                : query.OrderByDescending(e => e.ExpenseDate),
            "amount" => request.SortDirection == "asc"
                ? query.OrderBy(e => e.Amount)
                : query.OrderByDescending(e => e.Amount),
            "category" => request.SortDirection == "asc"
                ? query.OrderBy(e => e.Category)
                : query.OrderByDescending(e => e.Category),
            _ => query.OrderByDescending(e => e.ExpenseDate)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var expenses = await expenseRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        // Get related data
        var siteIds = expenses.Where(e => e.SiteId.HasValue).Select(e => e.SiteId!.Value).Distinct().ToList();
        var guardIds = expenses.Where(e => e.GuardId.HasValue).Select(e => e.GuardId!.Value).Distinct().ToList();

        var sites = siteIds.Any()
            ? await _unitOfWork.Repository<Site>().FindAsync(s => siteIds.Contains(s.Id), cancellationToken)
            : new List<Site>();

        var guards = guardIds.Any()
            ? await _unitOfWork.Repository<SecurityGuard>().FindAsync(g => guardIds.Contains(g.Id), cancellationToken)
            : new List<SecurityGuard>();

        var expenseDtos = expenses.Select(e => new ExpenseDto
        {
            Id = e.Id,
            ExpenseNumber = e.ExpenseNumber,
            ExpenseDate = e.ExpenseDate,
            Category = e.Category,
            Description = e.Description,
            Amount = e.Amount,
            PaymentMethod = e.PaymentMethod,
            VendorName = e.VendorName,
            SiteId = e.SiteId,
            SiteName = sites.FirstOrDefault(s => s.Id == e.SiteId)?.SiteName,
            GuardId = e.GuardId,
            GuardName = e.GuardId.HasValue 
                ? $"{guards.FirstOrDefault(g => g.Id == e.GuardId)?.FirstName} {guards.FirstOrDefault(g => g.Id == e.GuardId)?.LastName}"
                : null,
            Status = e.Status,
            IsActive = e.IsActive,
            CreatedDate = e.CreatedDate
        }).ToList();

        var response = new ExpenseListResponseDto
        {
            Items = expenseDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<ExpenseListResponseDto>.SuccessResponse(response, "Expenses retrieved successfully");
    }
}
