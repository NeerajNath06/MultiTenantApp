using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogSummaryBySite;

public class GetVehicleLogSummaryBySiteQueryHandler : IRequestHandler<GetVehicleLogSummaryBySiteQuery, ApiResponse<VehicleLogSummaryBySiteResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetVehicleLogSummaryBySiteQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<VehicleLogSummaryBySiteResponseDto>> Handle(GetVehicleLogSummaryBySiteQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<VehicleLogSummaryBySiteResponseDto>.ErrorResponse("Tenant context not found");

        var tenantId = _tenantContext.TenantId.Value;
        var dateFrom = request.DateFrom ?? DateTime.UtcNow.Date;
        var dateToEnd = request.DateTo.HasValue ? request.DateTo.Value.Date.AddDays(1) : dateFrom.AddDays(1);

        var allLogs = (await _unitOfWork.Repository<VehicleLog>().FindAsync(
            v => v.TenantId == tenantId &&
                 v.EntryTime >= dateFrom && v.EntryTime < dateToEnd &&
                 (!request.SiteId.HasValue || v.SiteId == request.SiteId.Value),
            cancellationToken)).ToList();

        var grouped = allLogs
            .GroupBy(v => v.SiteId)
            .Select(g => new
            {
                SiteId = g.Key,
                TotalEntries = g.Count(),
                InsideCount = g.Count(v => v.ExitTime == null),
                ExitedCount = g.Count(v => v.ExitTime != null)
            })
            .ToList();

        var siteIds = grouped.Select(x => x.SiteId).Distinct().ToList();
        var sites = siteIds.Any()
            ? (await _unitOfWork.Repository<Site>().FindAsync(s => siteIds.Contains(s.Id), cancellationToken)).ToList()
            : new List<Site>();
        var siteMap = sites.ToDictionary(s => s.Id, s => s);

        var list = grouped.Select(g =>
        {
            var site = siteMap.GetValueOrDefault(g.SiteId);
            return new VehicleLogSiteSummaryDto
            {
                SiteId = g.SiteId,
                SiteName = site?.SiteName ?? "Unknown",
                SiteAddress = site != null ? string.Join(", ", new[] { site.Address, site.City, site.State }.Where(x => !string.IsNullOrWhiteSpace(x))) : null,
                TotalEntries = g.TotalEntries,
                InsideCount = g.InsideCount,
                ExitedCount = g.ExitedCount
            };
        }).OrderByDescending(x => x.TotalEntries).ToList();

        var response = new VehicleLogSummaryBySiteResponseDto { Sites = list };
        return ApiResponse<VehicleLogSummaryBySiteResponseDto>.SuccessResponse(response, "Vehicle log summary by site");
    }
}
