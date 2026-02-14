using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Expenses.Commands.CreateExpense;

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateExpenseCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
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

        // Validate guard if provided
        if (request.GuardId.HasValue)
        {
            var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId.Value, cancellationToken);
            if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<Guid>.ErrorResponse("Invalid security guard");
            }
        }

        // Check if expense number exists
        var expenseRepo = _unitOfWork.Repository<Expense>();
        var existing = await expenseRepo.FirstOrDefaultAsync(
            e => e.ExpenseNumber == request.ExpenseNumber && e.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<Guid>.ErrorResponse("Expense number already exists");
        }

        var expense = new Expense
        {
            TenantId = _tenantContext.TenantId.Value,
            ExpenseNumber = request.ExpenseNumber,
            ExpenseDate = request.ExpenseDate,
            Category = request.Category,
            Description = request.Description,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            VendorName = request.VendorName,
            ReceiptNumber = request.ReceiptNumber,
            SiteId = request.SiteId,
            GuardId = request.GuardId,
            Status = request.Status,
            Notes = request.Notes,
            IsActive = true
        };

        await expenseRepo.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(expense.Id, "Expense created successfully");
    }
}
