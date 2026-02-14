using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class WagesController : Controller
{
    private readonly IApiClient _apiClient;

    public WagesController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? status = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["includeInactive"] = "false"
        };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (!string.IsNullOrEmpty(status)) query["status"] = status;
        var result = await _apiClient.GetAsync<WageListResponse>("api/v1/Wages", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new WageListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();
        var endDate = DateTime.UtcNow;
        var startDate = new DateTime(endDate.Year, endDate.Month, 1);
        var periodEnd = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
        return View(new CreateWageRequest
        {
            WagePeriodStart = startDate,
            WagePeriodEnd = periodEnd,
            PaymentDate = endDate.AddDays(7),
            Status = "Draft"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWageRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }
        var body = new
        {
            wageSheetNumber = request.WageSheetNumber,
            wagePeriodStart = request.WagePeriodStart,
            wagePeriodEnd = request.WagePeriodEnd,
            paymentDate = request.PaymentDate,
            status = request.Status,
            totalAllowances = request.TotalAllowances,
            totalDeductions = request.TotalDeductions,
            notes = request.Notes,
            wageDetails = (request.WageDetails ?? new List<WageDetailItemRequest>()).Select(d => new { guardId = d.GuardId, siteId = d.SiteId, shiftId = d.ShiftId, daysWorked = d.DaysWorked, hoursWorked = d.HoursWorked, basicRate = d.BasicRate, overtimeHours = d.OvertimeHours, overtimeRate = d.OvertimeRate, allowances = d.Allowances, deductions = d.Deductions, remarks = d.Remarks })
        };
        var result = await _apiClient.PostAsync<Guid>("api/v1/Wages", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Wage sheet created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDropdowns();
        return View(request);
    }

    private async Task LoadDropdowns()
    {
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = guardResult.Data?.Items ?? new List<GuardItemDto>();
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = siteResult.Data?.Items ?? new List<SiteDto>();
        var shiftResult = await _apiClient.GetAsync<ShiftListResponse>("api/v1/Shifts", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Shifts = shiftResult.Data?.Items ?? new List<ShiftItemDto>();
    }
}
