using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class MonthlyDashboardController : Controller
{
    private readonly IApiClient _apiClient;

    public MonthlyDashboardController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int? year = null, int? month = null)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var m = month ?? DateTime.UtcNow.Month;
        var res = await _apiClient.GetAsync<MonthlySiteSummaryResponseDto>("api/v1/Dashboard/monthly-summary",
            new Dictionary<string, string?> { ["year"] = y.ToString(), ["month"] = m.ToString() });
        if (!res.Success || res.Data == null)
            return View(new MonthlySiteSummaryResponseDto { Year = y, Month = m, Items = new List<MonthlySiteSummaryItemDto>() });
        return View(res.Data);
    }
}

