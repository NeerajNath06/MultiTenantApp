using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class DepartmentsController : Controller
{
    private readonly IApiClient _apiClient;

    public DepartmentsController(IApiClient apiClient)
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

        var result = await _apiClient.GetAsync<DepartmentListResponse>("api/v1/Departments", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new DepartmentListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateDepartmentRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDepartmentRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _apiClient.PostAsync<Guid>("api/v1/Departments", new { name = request.Name, code = request.Code, description = request.Description });
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Department created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var listResult = await _apiClient.GetAsync<DepartmentListResponse>("api/v1/Departments", new Dictionary<string, string?> { ["includeInactive"] = "true", ["pageSize"] = "1000" });
        var department = listResult.Data?.Items?.FirstOrDefault(d => d.Id == id);
        if (department == null)
            return NotFound();

        return View(new UpdateDepartmentRequest
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Description = department.Description,
            IsActive = department.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateDepartmentRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _apiClient.PutAsync<object>($"api/v1/Departments/{request.Id}", new { id = request.Id, name = request.Name, code = request.Code, description = request.Description, isActive = request.IsActive });
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Department updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/Departments/{id}");
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Department deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
