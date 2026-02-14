using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private readonly IApiClient _apiClient;

    public AttendanceController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(Guid? guardId = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null, int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = "desc")
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["startDate"] = (startDate ?? DateTime.Today.AddDays(-30)).ToString("yyyy-MM-dd"),
            ["endDate"] = (endDate ?? DateTime.Today).ToString("yyyy-MM-dd")
        };
        if (guardId.HasValue) query["guardId"] = guardId.Value.ToString();
        if (!string.IsNullOrEmpty(status)) query["status"] = status;
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (!string.IsNullOrEmpty(sortBy)) query["sortBy"] = sortBy;
        if (!string.IsNullOrEmpty(sortDirection)) query["sortDirection"] = sortDirection;
        var result = await _apiClient.GetAsync<AttendanceListResponse>("api/v1/Attendance", query);
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = new SelectList(
            guardResult.Data?.Items ?? new List<GuardItemDto>(),
            "Id", "GuardCode", guardId);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new AttendanceListResponse());
    }

    public async Task<IActionResult> Mark()
    {
        await LoadDropdowns();
        ViewBag.StatusList = new SelectList(new[] { new { Value = "Present", Text = "Present" }, new { Value = "Absent", Text = "Absent" }, new { Value = "Leave", Text = "Leave" }, new { Value = "HalfDay", Text = "Half Day" } }, "Value", "Text");
        return View(new MarkAttendanceRequest { AttendanceDate = DateTime.Today, Status = "Present" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mark(MarkAttendanceRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }
        var body = new
        {
            guardId = request.GuardId,
            assignmentId = request.AssignmentId,
            attendanceDate = request.AttendanceDate,
            checkInTime = request.CheckInTime,
            checkOutTime = request.CheckOutTime,
            checkInLocation = request.CheckInLocation,
            checkOutLocation = request.CheckOutLocation,
            status = request.Status,
            remarks = request.Remarks
        };
        var result = await _apiClient.PostAsync<Guid>("api/v1/Attendance/mark", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Attendance marked successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDropdowns();
        ViewBag.StatusList = new SelectList(new[] { new { Value = "Present", Text = "Present" }, new { Value = "Absent", Text = "Absent" }, new { Value = "Leave", Text = "Leave" }, new { Value = "HalfDay", Text = "Half Day" } }, "Value", "Text");
        return View(request);
    }

    private async Task LoadDropdowns()
    {
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = new SelectList(guardResult.Data?.Items ?? new List<GuardItemDto>(), "Id", "GuardCode");
        var assignmentResult = await _apiClient.GetAsync<AssignmentListResponse>("api/v1/GuardAssignments", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Assignments = new SelectList(assignmentResult.Data?.Items ?? new List<AssignmentItemDto>(), "Id", "SiteName");
        if (ViewBag.StatusList == null)
            ViewBag.StatusList = new SelectList(new[] { new { Value = "Present", Text = "Present" }, new { Value = "Absent", Text = "Absent" }, new { Value = "Leave", Text = "Leave" }, new { Value = "HalfDay", Text = "Half Day" } }, "Value", "Text");
    }
}
