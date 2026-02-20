using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Wages.Commands.CreateWage;

public class CreateWageCommandHandler : IRequestHandler<CreateWageCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateWageCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateWageCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        if (request.WageDetails == null || request.WageDetails.Count == 0)
        {
            return ApiResponse<Guid>.ErrorResponse("At least one wage detail is required");
        }

        if (request.WagePeriodEnd <= request.WagePeriodStart)
        {
            return ApiResponse<Guid>.ErrorResponse("Wage period end date must be after start date");
        }

        decimal totalWages = 0;
        decimal totalAllowances = 0;
        decimal totalDeductions = 0;

        // Validate guards and calculate totals
        foreach (var detailDto in request.WageDetails)
        {
            var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(detailDto.GuardId, cancellationToken);
            if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<Guid>.ErrorResponse($"Security guard not found: {detailDto.GuardId}");
            }

            // Validate site if provided
            if (detailDto.SiteId.HasValue)
            {
                var site = await _unitOfWork.Repository<Site>().GetByIdAsync(detailDto.SiteId.Value, cancellationToken);
                if (site == null || site.TenantId != _tenantContext.TenantId.Value)
                {
                    return ApiResponse<Guid>.ErrorResponse($"Invalid site: {detailDto.SiteId}");
                }
            }

            // Validate shift if provided
            if (detailDto.ShiftId.HasValue)
            {
                var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(detailDto.ShiftId.Value, cancellationToken);
                if (shift == null || shift.TenantId != _tenantContext.TenantId.Value)
                {
                    return ApiResponse<Guid>.ErrorResponse($"Invalid shift: {detailDto.ShiftId}");
                }
            }

            // Calculate amounts
            var basicAmount = detailDto.HoursWorked * detailDto.BasicRate;
            var overtimeAmount = detailDto.OvertimeHours * detailDto.OvertimeRate;
            var grossAmount = basicAmount + overtimeAmount + detailDto.Allowances;
            var netAmount = grossAmount - detailDto.Deductions;

            totalWages += grossAmount;
            totalAllowances += detailDto.Allowances;
            totalDeductions += detailDto.Deductions;
        }

        var wage = new Wage
        {
            TenantId = _tenantContext.TenantId.Value,
            CreatedBy = _currentUserService.UserId,
            WageSheetNumber = request.WageSheetNumber,
            WagePeriodStart = request.WagePeriodStart,
            WagePeriodEnd = request.WagePeriodEnd,
            PaymentDate = request.PaymentDate,
            Status = request.Status,
            TotalWages = totalWages,
            TotalAllowances = request.TotalAllowances + totalAllowances,
            TotalDeductions = request.TotalDeductions + totalDeductions,
            NetAmount = totalWages + request.TotalAllowances + totalAllowances - request.TotalDeductions - totalDeductions,
            Notes = request.Notes,
            IsActive = true
        };

        await _unitOfWork.Repository<Wage>().AddAsync(wage, cancellationToken);

        // Add wage details
        foreach (var detailDto in request.WageDetails)
        {
            var basicAmount = detailDto.HoursWorked * detailDto.BasicRate;
            var overtimeAmount = detailDto.OvertimeHours * detailDto.OvertimeRate;
            var grossAmount = basicAmount + overtimeAmount + detailDto.Allowances;
            var netAmount = grossAmount - detailDto.Deductions;

            var wageDetail = new WageDetail
            {
                WageId = wage.Id,
                GuardId = detailDto.GuardId,
                SiteId = detailDto.SiteId,
                ShiftId = detailDto.ShiftId,
                DaysWorked = detailDto.DaysWorked,
                HoursWorked = detailDto.HoursWorked,
                BasicRate = detailDto.BasicRate,
                BasicAmount = basicAmount,
                OvertimeHours = detailDto.OvertimeHours,
                OvertimeRate = detailDto.OvertimeRate,
                OvertimeAmount = overtimeAmount,
                Allowances = detailDto.Allowances,
                Deductions = detailDto.Deductions,
                GrossAmount = grossAmount,
                NetAmount = netAmount,
                Remarks = detailDto.Remarks
            };

            await _unitOfWork.Repository<WageDetail>().AddAsync(wageDetail, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(wage.Id, "Wage sheet created successfully");
    }
}
