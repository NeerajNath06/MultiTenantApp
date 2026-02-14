using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class PatrolScansController : Controller
{
    private readonly IApiClient _apiClient;

    public PatrolScansController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(Guid? guardId = null, Guid? siteId = null, DateTime? dateFrom = null, DateTime? dateTo = null, int pageNumber = 1, int pageSize = 20)
    {
        if (!guardId.HasValue)
        {
            await LoadDropdowns(null);
            return View(new PatrolScanListResponse { Items = new List<PatrolScanItemDto>() });
        }
        var query = new Dictionary<string, string?>
        {
            ["guardId"] = guardId.Value.ToString(),
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString()
        };
        if (siteId.HasValue) query["siteId"] = siteId.Value.ToString();
        if (dateFrom.HasValue) query["dateFrom"] = dateFrom.Value.ToString("O");
        if (dateTo.HasValue) query["dateTo"] = dateTo.Value.ToString("O");

        var result = await _apiClient.GetAsync<PatrolScanListResponse>("api/v1/PatrolScans", query);
        await LoadDropdowns(guardId);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new PatrolScanListResponse());
    }

    private async Task LoadDropdowns(Guid? selectedGuardId)
    {
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = new SelectList(guardResult.Data?.Items ?? new List<GuardItemDto>(), "Id", "GuardCode", selectedGuardId);
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = new SelectList(siteResult.Data?.Items ?? new List<SiteDto>(), "Id", "SiteName");
    }
}
