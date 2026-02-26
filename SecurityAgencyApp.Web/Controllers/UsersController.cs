using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly IApiClient _apiClient;

    public UsersController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private async Task LoadDropdownsAsync()
    {
        var dept = await _apiClient.GetAsync<DepartmentListResponse>("api/v1/Departments", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Departments = new SelectList(dept.Data?.Items ?? new List<DepartmentDto>(), "Id", "Name");
        var desig = await _apiClient.GetAsync<DesignationListResponse>("api/v1/Designations", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Designations = new SelectList(desig.Data?.Items ?? new List<DesignationDto>(), "Id", "Name");
        var rolesRes = await _apiClient.GetAsync<RoleListResponse>("api/v1/Roles", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "500" });
        ViewBag.Roles = rolesRes.Data?.Items ?? new List<RoleDto>();
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null)
    {
        var query = new Dictionary<string, string?> { ["pageNumber"] = pageNumber.ToString(), ["pageSize"] = pageSize.ToString() };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        var result = await _apiClient.GetAsync<UserListResponse>("api/v1/Users", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new UserListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();
        return View(new CreateUserRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(request);
        }
        var result = await _apiClient.PostAsync<Guid>("api/v1/Users", new { userName = request.UserName, email = request.Email, password = request.Password, firstName = request.FirstName, lastName = request.LastName, phoneNumber = request.PhoneNumber, departmentId = request.DepartmentId, designationId = request.DesignationId, roleIds = request.RoleIds });
        if (result.Success)
        {
            TempData["SuccessMessage"] = "User created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDropdownsAsync();
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<UserDetailDto>($"api/v1/Users/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        await LoadDropdownsAsync();
        var d = result.Data;
        return View(new UpdateUserRequest { Id = d.Id, FirstName = d.FirstName ?? "", LastName = d.LastName, Email = d.Email, PhoneNumber = d.PhoneNumber, AadharNumber = d.AadharNumber, PANNumber = d.PANNumber, UAN = d.UAN, DepartmentId = d.DepartmentId, DesignationId = d.DesignationId, IsActive = d.IsActive, RoleIds = d.RoleIds ?? new List<Guid>() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(request);
        }
        var result = await _apiClient.PutAsync<object>($"api/v1/Users/{request.Id}", new { id = request.Id, firstName = request.FirstName, lastName = request.LastName, email = request.Email, phoneNumber = request.PhoneNumber, aadharNumber = request.AadharNumber, panNumber = request.PANNumber, uan = request.UAN, departmentId = request.DepartmentId, designationId = request.DesignationId, isActive = request.IsActive, roleIds = request.RoleIds });
        if (result.Success)
        {
            TempData["SuccessMessage"] = "User updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDropdownsAsync();
        return View(request);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _apiClient.GetAsync<UserDetailDto>($"api/v1/Users/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/Users/{id}");
        if (result.Success)
        {
            TempData["SuccessMessage"] = "User deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = result.Message ?? "Failed to delete user.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
