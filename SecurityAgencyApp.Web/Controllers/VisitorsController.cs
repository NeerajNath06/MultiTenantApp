using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class VisitorsController : Controller
{
    private readonly IApiClient _apiClient;

    public VisitorsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? siteId = null, Guid? guardId = null, DateTime? dateFrom = null, DateTime? dateTo = null, bool? insideOnly = null, string? sortBy = null, string? sortDirection = "desc")
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString()
        };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (siteId.HasValue) query["siteId"] = siteId.Value.ToString();
        if (guardId.HasValue) query["guardId"] = guardId.Value.ToString();
        if (dateFrom.HasValue) query["dateFrom"] = dateFrom.Value.ToString("O");
        if (dateTo.HasValue) query["dateTo"] = dateTo.Value.ToString("O");
        if (insideOnly.HasValue) query["insideOnly"] = insideOnly.Value.ToString().ToLowerInvariant();
        if (!string.IsNullOrEmpty(sortBy)) query["sortBy"] = sortBy;
        if (!string.IsNullOrEmpty(sortDirection)) query["sortDirection"] = sortDirection;

        var result = await _apiClient.GetAsync<VisitorListResponse>("api/v1/Visitors", query);
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = new SelectList(siteResult.Data?.Items ?? new List<SiteDto>(), "Id", "SiteName", siteId);
        ViewBag.Guards = new SelectList(guardResult.Data?.Items ?? new List<GuardItemDto>(), "Id", "GuardCode", guardId);

        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new VisitorListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();
        return View(new CreateVisitorRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateVisitorRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }
        var body = new
        {
            visitorName = request.VisitorName,
            visitorType = request.VisitorType,
            companyName = request.CompanyName,
            phoneNumber = request.PhoneNumber,
            email = request.Email,
            purpose = request.Purpose,
            hostName = request.HostName,
            hostDepartment = request.HostDepartment,
            siteId = request.SiteId,
            guardId = request.GuardId,
            idProofType = request.IdProofType,
            idProofNumber = request.IdProofNumber
        };
        var result = await _apiClient.PostAsync<object>("api/v1/Visitors", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Visitor registered successfully. Badge can be issued at the gate.";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message ?? "Failed to register visitor.");
        await LoadDropdowns();
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkExit(Guid id)
    {
        var result = await _apiClient.PatchAsync<object>($"api/v1/Visitors/{id}/exit", new { exitTime = (DateTime?)null });
        if (result.Success)
            TempData["SuccessMessage"] = "Visitor exit recorded.";
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
