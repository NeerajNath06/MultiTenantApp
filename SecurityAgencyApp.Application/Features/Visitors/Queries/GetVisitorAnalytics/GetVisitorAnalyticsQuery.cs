using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorAnalytics;

public class GetVisitorAnalyticsQuery : IRequest<ApiResponse<VisitorAnalyticsDto>>
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public Guid? SiteId { get; set; }
    /// <summary>When set, return analytics only for sites this supervisor is assigned to.</summary>
    public Guid? SupervisorId { get; set; }
}

public class VisitorAnalyticsDto
{
    public int TotalVisitors { get; set; }
    public int CurrentlyInside { get; set; }
    public int AvgDurationMinutes { get; set; }
    public int PeakHour { get; set; }
    public List<VisitorPurposeDto> ByPurpose { get; set; } = new();
    public List<VisitorHourlyDto> ByHour { get; set; } = new();
    public List<VisitorHostDto> TopHosts { get; set; } = new();
}

public class VisitorPurposeDto
{
    public string Purpose { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Percentage { get; set; }
}

public class VisitorHourlyDto
{
    public int Hour { get; set; }
    public string HourLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class VisitorHostDto
{
    public string HostName { get; set; } = string.Empty;
    public int VisitorCount { get; set; }
}
