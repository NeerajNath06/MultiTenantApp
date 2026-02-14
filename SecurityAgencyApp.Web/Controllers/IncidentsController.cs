using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class IncidentsController : Controller
{
    private readonly IApiClient _apiClient;

    public IncidentsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, Guid? siteId = null, string? status = null, string? search = null, string? sortBy = null, string? sortDirection = "desc")
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString()
        };
        if (siteId.HasValue) query["siteId"] = siteId.Value.ToString();
        if (!string.IsNullOrEmpty(status)) query["status"] = status;
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (!string.IsNullOrEmpty(sortBy)) query["sortBy"] = sortBy;
        if (!string.IsNullOrEmpty(sortDirection)) query["sortDirection"] = sortDirection;
        var result = await _apiClient.GetAsync<IncidentListResponse>("api/v1/Incidents", query);
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = new SelectList(siteResult.Data?.Items ?? new List<SiteDto>(), "Id", "SiteName", siteId);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new IncidentListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();
        ViewBag.SeverityList = new SelectList(new[] { new { Value = "Low", Text = "Low" }, new { Value = "Medium", Text = "Medium" }, new { Value = "High", Text = "High" }, new { Value = "Critical", Text = "Critical" } }, "Value", "Text");
        return View(new CreateIncidentRequest { IncidentDate = DateTime.Now, Severity = "Medium" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateIncidentRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }
        var body = new { siteId = request.SiteId, guardId = request.GuardId, incidentDate = request.IncidentDate, incidentType = request.IncidentType, severity = request.Severity, description = request.Description, actionTaken = request.ActionTaken };
        var result = await _apiClient.PostAsync<Guid>("api/v1/Incidents", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Incident report created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDropdowns();
        ViewBag.SeverityList = new SelectList(new[] { new { Value = "Low", Text = "Low" }, new { Value = "Medium", Text = "Medium" }, new { Value = "High", Text = "High" }, new { Value = "Critical", Text = "Critical" } }, "Value", "Text");
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(Guid id, string? actionTaken)
    {
        var body = new { id, actionTaken, status = "Resolved" };
        var result = await _apiClient.PutAsync<object>($"api/v1/Incidents/{id}", body);
        if (result.Success)
            TempData["SuccessMessage"] = "Incident resolved successfully";
        else
            TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropdowns()
    {
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = new SelectList(siteResult.Data?.Items ?? new List<SiteDto>(), "Id", "SiteName");
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = new SelectList(guardResult.Data?.Items ?? new List<GuardItemDto>(), "Id", "GuardCode");
    }
}
