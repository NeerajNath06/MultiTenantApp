using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Wages.Queries.GetWageWithDetails;

public class GetWageWithDetailsQueryHandler : IRequestHandler<GetWageWithDetailsQuery, ApiResponse<WageWithDetailsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetWageWithDetailsQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<WageWithDetailsDto>> Handle(GetWageWithDetailsQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<WageWithDetailsDto>.ErrorResponse("Tenant context not found");

        var wage = await _unitOfWork.Repository<Wage>().GetByIdAsync(request.WageId, cancellationToken);
        if (wage == null || wage.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<WageWithDetailsDto>.ErrorResponse("Wage not found");

        var details = await _unitOfWork.Repository<WageDetail>().FindAsync(d => d.WageId == request.WageId, cancellationToken);
        var guardIds = details.Select(d => d.GuardId).Distinct().ToList();
        var guards = guardIds.Any()
            ? (await _unitOfWork.Repository<SecurityGuard>().FindAsync(g => guardIds.Contains(g.Id), cancellationToken)).ToDictionary(g => g.Id)
            : new Dictionary<Guid, SecurityGuard>();

        var detailDtos = details.OrderBy(d => d.CreatedDate).Select((d, i) =>
        {
            var guard = guards.GetValueOrDefault(d.GuardId);
            return new WageDetailRowDto
            {
                Id = d.Id,
                GuardId = d.GuardId,
                GuardName = guard != null ? $"{guard.FirstName} {guard.LastName}".Trim() : "",
                Designation = guard?.Designation,
                UAN = guard?.UAN,
                BankAccountNumber = guard?.BankAccountNumber,
                IFSCCode = guard?.IFSCCode,
                EmployeeCode = guard?.EmployeeCode,
                DaysWorked = d.DaysWorked,
                BasicRate = d.BasicRate,
                BasicAmount = d.BasicAmount,
                OvertimeHours = d.OvertimeHours,
                OvertimeAmount = d.OvertimeAmount,
                Allowances = d.Allowances,
                Deductions = d.Deductions,
                GrossAmount = d.GrossAmount,
                NetAmount = d.NetAmount
            };
        }).ToList();

        var dto = new WageWithDetailsDto
        {
            Id = wage.Id,
            WageSheetNumber = wage.WageSheetNumber,
            WagePeriodStart = wage.WagePeriodStart,
            WagePeriodEnd = wage.WagePeriodEnd,
            NetAmount = wage.NetAmount,
            Details = detailDtos
        };
        return ApiResponse<WageWithDetailsDto>.SuccessResponse(dto, "Wage with details retrieved");
    }
}
