using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorAnalytics;

public class GetVisitorAnalyticsQueryHandler : IRequestHandler<GetVisitorAnalyticsQuery, ApiResponse<VisitorAnalyticsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetVisitorAnalyticsQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<VisitorAnalyticsDto>> Handle(GetVisitorAnalyticsQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<VisitorAnalyticsDto>.ErrorResponse("Tenant context not found");

        var dateFrom = request.DateFrom ?? DateTime.UtcNow.Date;
        var dateTo = request.DateTo ?? DateTime.UtcNow.Date;
        var endOfDay = dateTo.Date.AddDays(1);

        var allVisitors = (await _unitOfWork.Repository<Visitor>().FindAsync(
            v => v.TenantId == _tenantContext.TenantId.Value && v.IsActive &&
                 v.EntryTime >= dateFrom && v.EntryTime < endOfDay &&
                 (request.SiteId == null || v.SiteId == request.SiteId.Value),
            cancellationToken)).ToList();

        if (request.SupervisorId.HasValue)
        {
            var supervisedSiteIds = (await _unitOfWork.Repository<SiteSupervisor>().FindAsync(
                ss => ss.UserId == request.SupervisorId.Value,
                cancellationToken)).Select(ss => ss.SiteId).Distinct().ToList();
            allVisitors = allVisitors.Where(v => supervisedSiteIds.Contains(v.SiteId)).ToList();
        }

        var total = allVisitors.Count;
        var currentlyInside = allVisitors.Count(v => v.ExitTime == null);

        var withDuration = allVisitors.Where(v => v.ExitTime.HasValue).ToList();
        var avgMinutes = withDuration.Count > 0
            ? (int)withDuration.Average(v => (v.ExitTime!.Value - v.EntryTime).TotalMinutes)
            : 0;

        var byHour = allVisitors.GroupBy(v => v.EntryTime.Hour)
            .OrderBy(g => g.Key)
            .Select(g => new VisitorHourlyDto { Hour = g.Key, HourLabel = g.Key + "AM", Count = g.Count() })
            .ToList();
        for (var i = 0; i < byHour.Count; i++)
        {
            var h = byHour[i].Hour;
            byHour[i].HourLabel = h <= 12 ? h + (h == 12 ? "PM" : "AM") : (h - 12) + "PM";
        }
        var peakHour = byHour.OrderByDescending(x => x.Count).FirstOrDefault()?.Hour ?? 0;

        var byPurpose = allVisitors
            .GroupBy(v => v.Purpose ?? "Other")
            .Select(g => new VisitorPurposeDto
            {
                Purpose = g.Key,
                Count = g.Count(),
                Percentage = total > 0 ? (int)Math.Round(g.Count() * 100.0 / total) : 0
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var topHosts = allVisitors
            .Where(v => !string.IsNullOrWhiteSpace(v.HostName))
            .GroupBy(v => v.HostName!)
            .Select(g => new VisitorHostDto { HostName = g.Key, VisitorCount = g.Count() })
            .OrderByDescending(x => x.VisitorCount)
            .Take(10)
            .ToList();

        var result = new VisitorAnalyticsDto
        {
            TotalVisitors = total,
            CurrentlyInside = currentlyInside,
            AvgDurationMinutes = avgMinutes,
            PeakHour = peakHour,
            ByPurpose = byPurpose,
            ByHour = byHour.OrderBy(x => x.Hour).ToList(),
            TopHosts = topHosts
        };

        return ApiResponse<VisitorAnalyticsDto>.SuccessResponse(result, "Analytics retrieved");
    }
}
