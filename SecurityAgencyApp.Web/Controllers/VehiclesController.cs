using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class VehiclesController : Controller
{
    private readonly IApiClient _apiClient;

    public VehiclesController(IApiClient apiClient)
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

        var result = await _apiClient.GetAsync<VehicleLogListResponse>("api/v1/Vehicles", query);
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = new SelectList(siteResult.Data?.Items ?? new List<SiteDto>(), "Id", "SiteName", siteId);
        ViewBag.Guards = new SelectList(guardResult.Data?.Items ?? new List<GuardItemDto>(), "Id", "GuardCode", guardId);

        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new VehicleLogListResponse());
    }

    public async Task<IActionResult> Summary(Guid? siteId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = new Dictionary<string, string?>();
        if (siteId.HasValue) query["siteId"] = siteId.Value.ToString();
        if (dateFrom.HasValue) query["dateFrom"] = dateFrom.Value.ToString("O");
        if (dateTo.HasValue) query["dateTo"] = dateTo.Value.ToString("O");

        var result = await _apiClient.GetAsync<VehicleLogSummaryBySiteResponse>("api/v1/Vehicles/summary", query.Count > 0 ? query : null);
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = new SelectList(siteResult.Data?.Items ?? new List<SiteDto>(), "Id", "SiteName", siteId);

        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new VehicleLogSummaryBySiteResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();
        return View(new CreateVehicleEntryRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateVehicleEntryRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }
        var body = new
        {
            vehicleNumber = request.VehicleNumber,
            vehicleType = request.VehicleType,
            driverName = request.DriverName,
            driverPhone = request.DriverPhone,
            purpose = request.Purpose,
            parkingSlot = request.ParkingSlot,
            siteId = request.SiteId,
            guardId = request.GuardId
        };
        var result = await _apiClient.PostAsync<object>("api/v1/Vehicles", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Vehicle entry registered successfully.";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message ?? "Failed to register vehicle entry.");
        await LoadDropdowns();
        return View(request);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _apiClient.GetAsync<VehicleLogDetailDto>($"api/v1/Vehicles/{id}");
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Message ?? "Vehicle log not found.";
            return RedirectToAction(nameof(Index));
        }
        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordExit(Guid id)
    {
        var result = await _apiClient.PatchAsync<object>($"api/v1/Vehicles/{id}/exit", new { exitTime = (DateTime?)null });
        if (result.Success)
            TempData["SuccessMessage"] = "Vehicle exit recorded.";
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
