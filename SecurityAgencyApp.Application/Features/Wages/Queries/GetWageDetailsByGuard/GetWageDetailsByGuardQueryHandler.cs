using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Wages.Queries.GetWageDetailsByGuard;

public class GetWageDetailsByGuardQueryHandler : IRequestHandler<GetWageDetailsByGuardQuery, ApiResponse<GuardPayslipsResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetWageDetailsByGuardQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<GuardPayslipsResponseDto>> Handle(GetWageDetailsByGuardQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<GuardPayslipsResponseDto>.ErrorResponse("Tenant context not found");

        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId, cancellationToken);
        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<GuardPayslipsResponseDto>.ErrorResponse("Guard not found");

        var detailRepo = _unitOfWork.Repository<WageDetail>();
        var wageRepo = _unitOfWork.Repository<Wage>();
        List<Guid> wageIdsInPeriod = new();
        if (request.PeriodStart.HasValue || request.PeriodEnd.HasValue)
        {
            var wageQuery = wageRepo.GetQueryable().Where(w => w.TenantId == _tenantContext.TenantId.Value);
            if (request.PeriodStart.HasValue)
                wageQuery = wageQuery.Where(w => w.WagePeriodEnd >= request.PeriodStart.Value);
            if (request.PeriodEnd.HasValue)
                wageQuery = wageQuery.Where(w => w.WagePeriodStart <= request.PeriodEnd.Value);
            wageIdsInPeriod = wageQuery.Select(w => w.Id).ToList();
        }

        var query = detailRepo.GetQueryable().Where(d => d.GuardId == request.GuardId);
        if (wageIdsInPeriod.Count > 0)
            query = query.Where(d => wageIdsInPeriod.Contains(d.WageId));
        query = query.OrderByDescending(d => d.CreatedDate);

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var details = await detailRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var wageIds = details.Select(d => d.WageId).Distinct().ToList();
        var wages = wageIds.Count > 0
            ? (await wageRepo.FindAsync(w => wageIds.Contains(w.Id), cancellationToken)).ToDictionary(w => w.Id)
            : new Dictionary<Guid, Wage>();

        var items = details.Select(d =>
        {
            var wage = wages.GetValueOrDefault(d.WageId);
            return new GuardPayslipDto
            {
                Id = d.Id,
                WageId = d.WageId,
                WageSheetNumber = wage?.WageSheetNumber ?? "",
                WagePeriodStart = wage?.WagePeriodStart ?? d.CreatedDate,
                WagePeriodEnd = wage?.WagePeriodEnd ?? d.CreatedDate,
                PaymentDate = wage?.PaymentDate ?? d.CreatedDate,
                Status = wage?.Status ?? "Draft",
                DaysWorked = d.DaysWorked,
                HoursWorked = d.HoursWorked,
                BasicAmount = d.BasicAmount,
                OvertimeAmount = d.OvertimeAmount,
                Allowances = d.Allowances,
                Deductions = d.Deductions,
                GrossAmount = d.GrossAmount,
                NetAmount = d.NetAmount,
                Remarks = d.Remarks
            };
        }).ToList();

        var response = new GuardPayslipsResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
        return ApiResponse<GuardPayslipsResponseDto>.SuccessResponse(response, "Payslips retrieved");
    }
}
