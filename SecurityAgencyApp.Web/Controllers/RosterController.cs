using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class RosterController : Controller
{
    private readonly IApiClient _apiClient;

    public RosterController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>Roster week view: deployments per day for date range.</summary>
    public async Task<IActionResult> Index(string? dateFrom = null, string? dateTo = null, Guid? siteId = null, Guid? supervisorId = null)
    {
        var today = DateTime.Today;
        if (string.IsNullOrEmpty(dateFrom) || !DateTime.TryParse(dateFrom, out var fromDate))
            fromDate = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (string.IsNullOrEmpty(dateTo) || !DateTime.TryParse(dateTo, out var toDate))
            toDate = fromDate.AddDays(6);

        if (fromDate > toDate)
            (fromDate, toDate) = (toDate, fromDate);

        var query = new Dictionary<string, string?>
        {
            ["dateFrom"] = fromDate.ToString("yyyy-MM-dd"),
            ["dateTo"] = toDate.ToString("yyyy-MM-dd")
        };
        if (siteId.HasValue) query["siteId"] = siteId.Value.ToString();
        if (supervisorId.HasValue) query["supervisorId"] = supervisorId.Value.ToString();

        var result = await _apiClient.GetAsync<RosterResponse>("api/v1/Deployments/roster", query);
        if (!result.Success || result.Data == null)
            return View(new RosterViewModel { DateFrom = fromDate, DateTo = toDate, Deployments = new List<RosterDeploymentDto>(), WeekDays = GetWeekDays(fromDate, toDate) });

        var data = result.Data;
        var weekDays = GetWeekDays(fromDate, toDate);
        var sitesResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = sitesResult.Data?.Items ?? new List<SiteDto>();

        return View(new RosterViewModel
        {
            DateFrom = fromDate,
            DateTo = toDate,
            Deployments = data.Deployments,
            WeekDays = weekDays,
            SiteId = siteId,
            SupervisorId = supervisorId
        });
    }

    private static List<DateTime> GetWeekDays(DateTime from, DateTime to)
    {
        var list = new List<DateTime>();
        for (var d = from; d <= to; d = d.AddDays(1))
            list.Add(d);
        return list;
    }
}

public class RosterViewModel
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<RosterDeploymentDto> Deployments { get; set; } = new();
    public List<DateTime> WeekDays { get; set; } = new();
    public Guid? SiteId { get; set; }
    public Guid? SupervisorId { get; set; }
}
