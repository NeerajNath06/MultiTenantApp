using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogSummaryBySite;

/// <summary>
/// For admin/supervisor: per-site vehicle log counts (total, inside, exited) in a date range.
/// When siteId is null, returns all sites with logs; when set, returns that site's summary only.
/// </summary>
public class GetVehicleLogSummaryBySiteQuery : IRequest<ApiResponse<VehicleLogSummaryBySiteResponseDto>>
{
    public Guid? SiteId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class VehicleLogSummaryBySiteResponseDto
{
    public List<VehicleLogSiteSummaryDto> Sites { get; set; } = new();
}

public class VehicleLogSiteSummaryDto
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string? SiteAddress { get; set; }
    public int TotalEntries { get; set; }
    public int InsideCount { get; set; }
    public int ExitedCount { get; set; }
}
