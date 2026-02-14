using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class DesignationsController : Controller
{
    private readonly IApiClient _apiClient;

    public DesignationsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private async Task LoadDepartmentsAsync()
    {
        var result = await _apiClient.GetAsync<DepartmentListResponse>("api/v1/Departments", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Departments = new SelectList(result.Data?.Items ?? new List<DepartmentDto>(), "Id", "Name");
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

        var result = await _apiClient.GetAsync<DesignationListResponse>("api/v1/Designations", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new DesignationListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadDepartmentsAsync();
        return View(new CreateDesignationRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDesignationRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync();
            return View(request);
        }
        var result = await _apiClient.PostAsync<Guid>("api/v1/Designations", new { name = request.Name, code = request.Code, departmentId = request.DepartmentId, description = request.Description });
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Designation created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDepartmentsAsync();
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<DesignationDto>($"api/v1/Designations/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        await LoadDepartmentsAsync();
        var d = result.Data;
        return View(new UpdateDesignationRequest { Id = d.Id, Name = d.Name, Code = d.Code, DepartmentId = d.DepartmentId, Description = d.Description, IsActive = d.IsActive });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateDesignationRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync();
            return View(request);
        }
        var result = await _apiClient.PutAsync<object>($"api/v1/Designations/{request.Id}", new { id = request.Id, name = request.Name, code = request.Code, departmentId = request.DepartmentId, description = request.Description, isActive = request.IsActive });
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Designation updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDepartmentsAsync();
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/Designations/{id}");
        if (result.Success)
            TempData["SuccessMessage"] = "Designation deleted successfully";
        else
            TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
