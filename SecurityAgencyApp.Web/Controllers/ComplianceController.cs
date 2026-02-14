using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class ComplianceController : Controller
{
    private readonly IApiClient _apiClient;

    public ComplianceController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var query = new Dictionary<string, string?>();
        if (string.Equals(HttpContext.Session.GetString("IsSupervisor"), "True", StringComparison.OrdinalIgnoreCase))
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr))
                query["supervisorId"] = userIdStr;
        }
        var result = await _apiClient.GetAsync<ComplianceSummaryResponse>("api/v1/Compliance/summary", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new ComplianceSummaryResponse());
    }
}
