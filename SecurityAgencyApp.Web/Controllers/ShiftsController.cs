using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class ShiftsController : Controller
{
    private readonly IApiClient _apiClient;

    public ShiftsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = "asc")
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["includeInactive"] = "false"
        };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (!string.IsNullOrEmpty(sortBy)) query["sortBy"] = sortBy;
        if (!string.IsNullOrEmpty(sortDirection)) query["sortDirection"] = sortDirection;
        var result = await _apiClient.GetAsync<ShiftListResponse>("api/v1/Shifts", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new ShiftListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateShiftRequest
        {
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(18, 0, 0)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateShiftRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);
        var body = new
        {
            shiftName = request.ShiftName,
            startTime = request.StartTime,
            endTime = request.EndTime,
            breakDuration = request.BreakDuration
        };
        var result = await _apiClient.PostAsync<Guid>("api/v1/Shifts", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Shift created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<ShiftDetailDto>($"api/v1/Shifts/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        var d = result.Data;
        return View(new UpdateShiftRequest
        {
            Id = d.Id,
            ShiftName = d.ShiftName,
            StartTime = d.StartTime,
            EndTime = d.EndTime,
            BreakDuration = d.BreakDuration,
            IsActive = d.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateShiftRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);
        var body = new
        {
            id = request.Id,
            shiftName = request.ShiftName,
            startTime = request.StartTime,
            endTime = request.EndTime,
            breakDuration = request.BreakDuration,
            isActive = request.IsActive
        };
        var result = await _apiClient.PutAsync<object>($"api/v1/Shifts/{request.Id}", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Shift updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/Shifts/{id}");
        if (result.Success)
            TempData["SuccessMessage"] = "Shift deleted successfully";
        else
            TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
