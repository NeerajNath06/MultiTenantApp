using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class GuardAssignmentsController : Controller
{
    private readonly IApiClient _apiClient;

    public GuardAssignmentsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(Guid? guardId = null, Guid? siteId = null, int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = "desc")
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["includeInactive"] = "false"
        };
        if (guardId.HasValue) query["guardId"] = guardId.Value.ToString();
        if (siteId.HasValue) query["siteId"] = siteId.Value.ToString();
        if (string.Equals(HttpContext.Session.GetString("IsSupervisor"), "True", StringComparison.OrdinalIgnoreCase))
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var supId))
                query["supervisorId"] = supId.ToString();
        }
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (!string.IsNullOrEmpty(sortBy)) query["sortBy"] = sortBy;
        if (!string.IsNullOrEmpty(sortDirection)) query["sortDirection"] = sortDirection;
        var result = await _apiClient.GetAsync<AssignmentListResponse>("api/v1/GuardAssignments", query);
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = new SelectList(guardResult.Data?.Items ?? new List<GuardItemDto>(), "Id", "GuardCode", guardId);
        ViewBag.Sites = new SelectList(siteResult.Data?.Items ?? new List<SiteDto>(), "Id", "SiteName", siteId);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new AssignmentListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();
        return View(new CreateAssignmentRequest { AssignmentStartDate = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAssignmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }
        var body = new
        {
            guardId = request.GuardId,
            siteId = request.SiteId,
            shiftId = request.ShiftId,
            supervisorId = request.SupervisorId,
            assignmentStartDate = request.AssignmentStartDate,
            assignmentEndDate = request.AssignmentEndDate,
            remarks = request.Remarks
        };
        var result = await _apiClient.PostAsync<Guid>("api/v1/GuardAssignments", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Guard assigned successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDropdowns();
        return View(request);
    }

    private async Task LoadDropdowns()
    {
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = new SelectList(guardResult.Data?.Items ?? new List<GuardItemDto>(), "Id", "GuardCode");
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = new SelectList(siteResult.Data?.Items ?? new List<SiteDto>(), "Id", "SiteName");
        var shiftResult = await _apiClient.GetAsync<ShiftListResponse>("api/v1/Shifts", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Shifts = new SelectList(shiftResult.Data?.Items ?? new List<ShiftItemDto>(), "Id", "ShiftName");
        var userResult = await _apiClient.GetAsync<UserListResponse>("api/v1/Users", new Dictionary<string, string?> { ["pageSize"] = "1000" });
        ViewBag.Supervisors = new SelectList(userResult.Data?.Items ?? new List<UserItemDto>(), "Id", "UserName");
    }
}
