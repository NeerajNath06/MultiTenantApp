using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Application.Features.Dashboard.Queries.GetMonthlySiteSummary;

public class GetMonthlySiteSummaryQueryHandler : IRequestHandler<GetMonthlySiteSummaryQuery, ApiResponse<MonthlySiteSummaryResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetMonthlySiteSummaryQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<MonthlySiteSummaryResponseDto>> Handle(GetMonthlySiteSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<MonthlySiteSummaryResponseDto>.ErrorResponse("Tenant context not found");
        if (request.Month < 1 || request.Month > 12)
            return ApiResponse<MonthlySiteSummaryResponseDto>.ErrorResponse("Invalid month");

        var start = new DateTime(request.Year, request.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        // Present attendance grouped by site (via assignment)
        var assignments = _unitOfWork.Repository<GuardAssignment>().GetQueryable();
        var attendance = _unitOfWork.Repository<GuardAttendance>().GetQueryable();

        var presentBySite = (from a in attendance
                             join ass in assignments on a.AssignmentId equals ass.Id
                             where a.AttendanceDate.Date >= start.Date &&
                                   a.AttendanceDate.Date <= end.Date &&
                                   a.Status == AttendanceStatus.Present
                             group a by ass.SiteId into g
                             select new { SiteId = g.Key, PresentCount = g.Count() })
            .ToList();

        var siteIds = presentBySite.Select(x => x.SiteId).Distinct().ToList();
        // Include sites that have bills/wages even if attendance is zero
        var bills = _unitOfWork.Repository<Bill>().GetQueryable()
            .Where(b => b.BillYear == request.Year && b.BillMonth == request.Month && b.SiteId != null)
            .ToList();
        var wages = _unitOfWork.Repository<Wage>().GetQueryable()
            .Where(w => w.WageYear == request.Year && w.WageMonth == request.Month && w.SiteId != null)
            .ToList();
        siteIds.AddRange(bills.Where(b => b.SiteId.HasValue).Select(b => b.SiteId!.Value));
        siteIds.AddRange(wages.Where(w => w.SiteId.HasValue).Select(w => w.SiteId!.Value));
        siteIds = siteIds.Distinct().ToList();

        var sites = siteIds.Count > 0
            ? (await _unitOfWork.Repository<Site>().FindAsync(s => siteIds.Contains(s.Id), cancellationToken)).ToDictionary(s => s.Id)
            : new Dictionary<Guid, Site>();

        var billLatestBySite = bills
            .Where(b => b.SiteId.HasValue)
            .GroupBy(b => b.SiteId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.BillDate).FirstOrDefault());

        var wageLatestBySite = wages
            .Where(w => w.SiteId.HasValue)
            .GroupBy(w => w.SiteId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.PaymentDate).FirstOrDefault());

        var presentDict = presentBySite.ToDictionary(x => x.SiteId, x => x.PresentCount);

        var items = siteIds
            .Select(id =>
            {
                var s = sites.GetValueOrDefault(id);
                var bill = billLatestBySite.GetValueOrDefault(id);
                var wage = wageLatestBySite.GetValueOrDefault(id);
                return new MonthlySiteSummaryItemDto
                {
                    SiteId = id,
                    SiteName = s?.SiteName ?? "",
                    ClientName = s?.ClientName ?? "",
                    PresentDaysTotal = presentDict.GetValueOrDefault(id, 0),
                    LatestBillTotal = bill?.TotalAmount,
                    LatestWageTotal = wage?.NetAmount
                };
            })
            .OrderBy(x => x.SiteName)
            .ToList();

        return ApiResponse<MonthlySiteSummaryResponseDto>.SuccessResponse(new MonthlySiteSummaryResponseDto
        {
            Year = request.Year,
            Month = request.Month,
            Items = items
        });
    }
}

