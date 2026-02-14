using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class LeaveRequestsController : Controller
{
    private readonly IApiClient _apiClient;

    public LeaveRequestsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? guardId = null, string? status = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["includeInactive"] = "false"
        };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (guardId.HasValue) query["guardId"] = guardId.Value.ToString();
        if (!string.IsNullOrEmpty(status)) query["status"] = status;
        var result = await _apiClient.GetAsync<LeaveRequestListResponse>("api/v1/LeaveRequests", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new LeaveRequestListResponse());
    }

    public async Task<IActionResult> Create()
    {
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = guardResult.Data?.Items ?? new List<GuardItemDto>();
        return View(new CreateLeaveRequestRequest { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1), LeaveType = "Casual" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLeaveRequestRequest request)
    {
        if (!ModelState.IsValid)
        {
            var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
            ViewBag.Guards = guardResult.Data?.Items ?? new List<GuardItemDto>();
            return View(request);
        }
        var body = new { guardId = request.GuardId, leaveType = request.LeaveType, startDate = request.StartDate, endDate = request.EndDate, reason = request.Reason, notes = request.Notes };
        var result = await _apiClient.PostAsync<Guid>("api/v1/LeaveRequests", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Leave request created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        var guardRes = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = guardRes.Data?.Items ?? new List<GuardItemDto>();
        return View(request);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(Guid id, bool isApproved, string? rejectionReason)
    {
        var body = new { leaveRequestId = id, isApproved, rejectionReason };
        var result = await _apiClient.PostAsync<object>($"api/v1/LeaveRequests/{id}/approve", body);
        if (result.Success)
            TempData["SuccessMessage"] = isApproved ? "Leave request approved" : "Leave request rejected";
        else
            TempData["ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
