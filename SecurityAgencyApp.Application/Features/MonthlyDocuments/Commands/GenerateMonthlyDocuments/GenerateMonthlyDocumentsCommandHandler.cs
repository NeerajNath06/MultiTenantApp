using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Application.Features.MonthlyDocuments.Commands.GenerateMonthlyDocuments;

public class GenerateMonthlyDocumentsCommandHandler : IRequestHandler<GenerateMonthlyDocumentsCommand, ApiResponse<GenerateMonthlyDocumentsResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GenerateMonthlyDocumentsCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<GenerateMonthlyDocumentsResultDto>> Handle(GenerateMonthlyDocumentsCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<GenerateMonthlyDocumentsResultDto>.ErrorResponse("Tenant context not found");
        if (request.Month < 1 || request.Month > 12)
            return ApiResponse<GenerateMonthlyDocumentsResultDto>.ErrorResponse("Invalid month");

        var periodStart = new DateTime(request.Year, request.Month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null)
            return ApiResponse<GenerateMonthlyDocumentsResultDto>.ErrorResponse("Site not found");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Resolve rate plan effective on period start; if none, use 0 so generation still runs
            var rateRepo = _unitOfWork.Repository<SiteRatePlan>();
            var plans = await rateRepo.FindAsync(r =>
                r.SiteId == request.SiteId &&
                r.IsActive &&
                r.EffectiveFrom.Date <= periodStart.Date &&
                (r.EffectiveTo == null || r.EffectiveTo.Value.Date >= periodStart.Date), cancellationToken);
            var plan = plans.OrderByDescending(p => p.EffectiveFrom).FirstOrDefault();
            decimal rate = plan?.RateAmount ?? 0m;
            decimal allowancePercent = plan?.AllowancePercent ?? 0m;
            decimal epfPercent = plan?.EpfPercent ?? 0m;
            decimal esicPercent = plan?.EsicPercent ?? 0m;
            decimal? epfWageCap = plan?.EpfWageCap;

            // Assignments overlapping period for this site
            var assignments = await _unitOfWork.Repository<GuardAssignment>().FindAsync(a =>
                a.SiteId == request.SiteId &&
                a.AssignmentStartDate.Date <= periodEnd.Date &&
                (a.AssignmentEndDate == null || a.AssignmentEndDate.Value.Date >= periodStart.Date), cancellationToken);
            var assignmentIds = assignments.Select(a => a.Id).Distinct().ToList();
            if (assignmentIds.Count == 0)
                return ApiResponse<GenerateMonthlyDocumentsResultDto>.ErrorResponse("No guard assignments found for this site in the selected month");

            // Attendance for those assignments (Present only billable/payable)
            var attRepo = _unitOfWork.Repository<GuardAttendance>();
            var query = attRepo.GetQueryable()
                .Where(a => assignmentIds.Contains(a.AssignmentId) &&
                            a.AttendanceDate.Date >= periodStart.Date &&
                            a.AttendanceDate.Date <= periodEnd.Date &&
                            a.Status == AttendanceStatus.Present);
            var presentAttendances = query.ToList();

            var presentByGuard = presentAttendances
                .GroupBy(a => a.GuardId)
                .ToDictionary(g => g.Key, g => g.Count());

            var guardIds = presentByGuard.Keys.ToList();
            var guards = guardIds.Count > 0
                ? (await _unitOfWork.Repository<SecurityGuard>().FindAsync(g => guardIds.Contains(g.Id), cancellationToken)).ToDictionary(g => g.Id)
                : new Dictionary<Guid, SecurityGuard>();

            // Create Bill (minimal header + auto items)
            var bill = new Bill
            {
                TenantId = _tenantContext.TenantId.Value,
                BillNumber = $"INV-{request.Year}{request.Month:D2}-{DateTime.UtcNow:HHmmss}",
                BillDate = DateTime.UtcNow,
                SiteId = site.Id,
                ClientId = site.ClientId,
                ClientName = site.ClientName ?? "",
                Description = $"Auto-generated bill for {site.SiteName} - {periodStart:MMM yyyy} (Present only)",
                BillYear = request.Year,
                BillMonth = request.Month,
                RateAmount = rate,
                Status = "Draft",
                Notes = $"Period: {request.Year}-{request.Month:D2}; Rate: {rate:N2}/day",
                IsActive = true
            };

            decimal subTotal = 0m;
            foreach (var kv in presentByGuard.OrderBy(k => k.Key))
            {
                var guard = guards.GetValueOrDefault(kv.Key);
                var guardName = guard != null ? $"{guard.FirstName} {guard.LastName}".Trim() : kv.Key.ToString();
                var qty = kv.Value; // present days
                var amount = rate * qty;
                subTotal += amount;

                bill.Items.Add(new BillItem
                {
                    ItemName = guardName,
                    Description = $"Present days: {qty} @ {rate:N2}/day",
                    Quantity = qty,
                    UnitPrice = rate,
                    TaxRate = 0,
                    TaxAmount = 0,
                    DiscountAmount = 0,
                    TotalAmount = amount
                });
            }
            bill.SubTotal = subTotal;
            bill.TaxAmount = 0;
            bill.DiscountAmount = 0;
            bill.TotalAmount = subTotal;

            await _unitOfWork.Repository<Bill>().AddAsync(bill, cancellationToken);

            // Create Wage sheet (Present only)
            var wage = new Wage
            {
                TenantId = _tenantContext.TenantId.Value,
                WageSheetNumber = $"WAGE-{request.Year}{request.Month:D2}-{DateTime.UtcNow:HHmmss}",
                SiteId = site.Id,
                WagePeriodStart = periodStart,
                WagePeriodEnd = periodEnd,
                PaymentDate = DateTime.UtcNow,
                WageYear = request.Year,
                WageMonth = request.Month,
                RateAmount = rate,
                Status = "Draft",
                Notes = $"Auto-generated for site {site.SiteName}; Rate: {rate:N2}/day",
                IsActive = true
            };

            decimal totalWages = 0m;
            decimal totalAllowances = 0m;
            decimal totalDeductions = 0m;
            foreach (var kv in presentByGuard.OrderBy(k => k.Key))
            {
                var guard = guards.GetValueOrDefault(kv.Key);
                var qty = kv.Value;
                var basicAmount = rate * qty;
                var allowanceAmount = RoundCurrency(basicAmount * allowancePercent / 100m);
                var epfBase = epfWageCap.HasValue && epfWageCap.Value > 0m
                    ? Math.Min(basicAmount, epfWageCap.Value)
                    : basicAmount;
                var epfAmount = RoundCurrency(epfBase * epfPercent / 100m);
                var grossAmount = basicAmount + allowanceAmount;
                var esicAmount = RoundCurrency(grossAmount * esicPercent / 100m);
                var deductionAmount = epfAmount + esicAmount;
                var netAmount = grossAmount - deductionAmount;

                totalWages += basicAmount;
                totalAllowances += allowanceAmount;
                totalDeductions += deductionAmount;

                wage.WageDetails.Add(new WageDetail
                {
                    GuardId = kv.Key,
                    SiteId = site.Id,
                    DaysWorked = qty,
                    HoursWorked = 0,
                    BasicRate = rate,
                    BasicAmount = basicAmount,
                    OvertimeHours = 0,
                    OvertimeRate = 0,
                    OvertimeAmount = 0,
                    Allowances = allowanceAmount,
                    Deductions = deductionAmount,
                    GrossAmount = grossAmount,
                    NetAmount = netAmount,
                    Remarks = BuildWageRemarks(guard, qty, allowancePercent, epfPercent, esicPercent, allowanceAmount, epfAmount, esicAmount)
                });
            }
            wage.TotalWages = totalWages;
            wage.TotalAllowances = totalAllowances;
            wage.TotalDeductions = totalDeductions;
            wage.NetAmount = totalWages + totalAllowances - totalDeductions;

            await _unitOfWork.Repository<Wage>().AddAsync(wage, cancellationToken);

            var payrollRun = new PayrollRun
            {
                TenantId = _tenantContext.TenantId.Value,
                BranchId = site.BranchId,
                SiteId = site.Id,
                WageId = wage.Id,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                PayrollYear = request.Year,
                PayrollMonth = request.Month,
                Status = "Draft",
                TotalGuards = presentByGuard.Count,
                GrossAmount = totalWages + totalAllowances,
                TotalDeductions = totalDeductions,
                NetAmount = wage.NetAmount,
                Notes = $"Auto-created from monthly document generation for {site.SiteName}"
            };

            await _unitOfWork.Repository<PayrollRun>().AddAsync(payrollRun, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return ApiResponse<GenerateMonthlyDocumentsResultDto>.SuccessResponse(new GenerateMonthlyDocumentsResultDto
            {
                BillId = bill.Id,
                WageId = wage.Id,
                PresentAttendanceCount = presentAttendances.Count,
                RateAmount = rate
            }, "Monthly documents generated");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApiResponse<GenerateMonthlyDocumentsResultDto>.ErrorResponse("Monthly generation failed: " + ex.Message);
        }
    }

    private static decimal RoundCurrency(decimal amount)
    {
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    private static string? BuildWageRemarks(
        SecurityGuard? guard,
        int daysWorked,
        decimal allowancePercent,
        decimal epfPercent,
        decimal esicPercent,
        decimal allowanceAmount,
        decimal epfAmount,
        decimal esicAmount)
    {
        var guardName = guard != null ? $"{guard.FirstName} {guard.LastName}".Trim() : null;
        var summary = $"Present days: {daysWorked}";

        if (allowancePercent > 0m || epfPercent > 0m || esicPercent > 0m)
        {
            summary += $"; Allowance {allowancePercent:N2}%={allowanceAmount:N2}; EPF {epfPercent:N2}%={epfAmount:N2}; ESIC {esicPercent:N2}%={esicAmount:N2}";
        }

        return string.IsNullOrWhiteSpace(guardName) ? summary : $"{guardName} | {summary}";
    }
}

