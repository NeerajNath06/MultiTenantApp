using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class RolesController : Controller
{
    private readonly IApiClient _apiClient;

    public RolesController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private async Task LoadPermissionsAsync()
    {
        var result = await _apiClient.GetAsync<PermissionListResponse>("api/v1/Permissions", new Dictionary<string, string?> { ["includeInactive"] = "false" });
        ViewBag.Permissions = result.Data?.Items ?? new List<PermissionDto>();
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

        var result = await _apiClient.GetAsync<RoleListResponse>("api/v1/Roles", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new RoleListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadPermissionsAsync();
        return View(new CreateRoleRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadPermissionsAsync();
            return View(request);
        }
        var result = await _apiClient.PostAsync<Guid>("api/v1/Roles", new { name = request.Name, code = request.Code, description = request.Description });
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Role created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadPermissionsAsync();
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<RoleDto>($"api/v1/Roles/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        await LoadPermissionsAsync();
        var d = result.Data;
        return View(new UpdateRoleRequest { Id = d.Id, Name = d.Name, Code = d.Code, Description = d.Description, IsActive = d.IsActive, PermissionIds = d.PermissionIds });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadPermissionsAsync();
            return View(request);
        }
        var result = await _apiClient.PutAsync<object>($"api/v1/Roles/{request.Id}", new { id = request.Id, name = request.Name, code = request.Code, description = request.Description, isActive = request.IsActive, permissionIds = request.PermissionIds });
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Role updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadPermissionsAsync();
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/Roles/{id}");
        if (result.Success)
            TempData["SuccessMessage"] = "Role deleted successfully";
        else
            TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ManagePermissions(Guid id)
    {
        var roleResult = await _apiClient.GetAsync<RoleDto>($"api/v1/Roles/{id}");
        if (!roleResult.Success || roleResult.Data == null)
            return NotFound();
        var permResult = await _apiClient.GetAsync<PermissionListResponse>("api/v1/Permissions", new Dictionary<string, string?> { ["includeInactive"] = "false" });
        ViewBag.Role = roleResult.Data;
        ViewBag.Permissions = permResult.Data?.Items ?? new List<PermissionDto>();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManagePermissions(Guid id, List<Guid> permissionIds)
    {
        var result = await _apiClient.PostAsync<object>($"api/v1/Roles/{id}/assign-permissions", permissionIds ?? new List<Guid>());
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Permissions assigned successfully";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(ManagePermissions), new { id });
    }

    public async Task<IActionResult> ManageMenus(Guid id)
    {
        var roleResult = await _apiClient.GetAsync<RoleDto>($"api/v1/Roles/{id}");
        if (!roleResult.Success || roleResult.Data == null)
            return NotFound();
        var menuResult = await _apiClient.GetAsync<MenuListResponse>("api/v1/Menus", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "100" });
        ViewBag.Role = roleResult.Data;
        ViewBag.Menus = menuResult.Data?.Items ?? new List<MenuDto>();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageMenus(Guid id, List<Guid> menuIds, List<Guid> subMenuIds)
    {
        var body = new { menuIds = menuIds ?? new List<Guid>(), subMenuIds = subMenuIds ?? new List<Guid>() };
        var result = await _apiClient.PostAsync<object>($"api/v1/Menus/{id}/assign", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Menus and SubMenus assigned successfully";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(ManageMenus), new { id });
    }
}
