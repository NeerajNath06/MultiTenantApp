using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Expenses.Queries.GetExpenseById;

public class GetExpenseByIdQueryHandler : IRequestHandler<GetExpenseByIdQuery, ApiResponse<ExpenseDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetExpenseByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<ExpenseDetailDto>> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<ExpenseDetailDto>.ErrorResponse("Tenant context not found");

        var expense = await _unitOfWork.Repository<Expense>().GetByIdAsync(request.Id, cancellationToken);
        if (expense == null || expense.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<ExpenseDetailDto>.ErrorResponse("Expense not found");

        string? siteName = null;
        string? guardName = null;
        if (expense.SiteId.HasValue)
        {
            var site = await _unitOfWork.Repository<Site>().GetByIdAsync(expense.SiteId.Value, cancellationToken);
            siteName = site?.SiteName;
        }
        if (expense.GuardId.HasValue)
        {
            var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(expense.GuardId.Value, cancellationToken);
            guardName = guard != null ? $"{guard.FirstName} {guard.LastName}".Trim() : null;
        }

        var dto = new ExpenseDetailDto
        {
            Id = expense.Id,
            ExpenseNumber = expense.ExpenseNumber,
            ExpenseDate = expense.ExpenseDate,
            Category = expense.Category,
            Description = expense.Description,
            Amount = expense.Amount,
            PaymentMethod = expense.PaymentMethod,
            VendorName = expense.VendorName,
            ReceiptNumber = expense.ReceiptNumber,
            SiteId = expense.SiteId,
            SiteName = siteName,
            GuardId = expense.GuardId,
            GuardName = guardName,
            Status = expense.Status,
            Notes = expense.Notes,
            IsActive = expense.IsActive,
            CreatedDate = expense.CreatedDate
        };
        return ApiResponse<ExpenseDetailDto>.SuccessResponse(dto, "Expense retrieved");
    }
}
