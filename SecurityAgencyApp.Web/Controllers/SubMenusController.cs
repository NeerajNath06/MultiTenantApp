using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class SubMenusController : Controller
{
    private readonly IApiClient _apiClient;

    public SubMenusController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(Guid? menuId = null)
    {
        var query = new Dictionary<string, string?> { ["includeInactive"] = "false" };
        if (menuId.HasValue) query["menuId"] = menuId.Value.ToString();
        var result = await _apiClient.GetAsync<SubMenuListResponse>("api/v1/SubMenus", query);
        var menuResult = await _apiClient.GetAsync<MenuListResponse>("api/v1/Menus", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "100" });
        ViewBag.Menus = menuResult.Data?.Items ?? new List<MenuDto>();
        ViewBag.SelectedMenuId = menuId;
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new SubMenuListResponse());
    }

    public IActionResult Create(Guid menuId)
    {
        return View(new CreateSubMenuRequest { MenuId = menuId, IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSubMenuRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);
        var result = await _apiClient.PostAsync<Guid>("api/v1/SubMenus", new { menuId = request.MenuId, name = request.Name, displayName = request.DisplayName, icon = request.Icon, route = request.Route, displayOrder = request.DisplayOrder, isActive = request.IsActive });
        if (result.Success)
        {
            TempData["SuccessMessage"] = "SubMenu created successfully";
            return RedirectToAction("Index", new { menuId = request.MenuId });
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var listResult = await _apiClient.GetAsync<SubMenuListResponse>("api/v1/SubMenus", new Dictionary<string, string?> { ["includeInactive"] = "true" });
        var subMenu = listResult.Data?.Items?.FirstOrDefault(sm => sm.Id == id);
        if (subMenu == null)
            return NotFound();
        return View(new UpdateSubMenuRequest { Id = subMenu.Id, MenuId = subMenu.MenuId, Name = subMenu.Name, DisplayName = subMenu.DisplayName, Icon = subMenu.Icon, Route = subMenu.Route, DisplayOrder = subMenu.DisplayOrder, IsActive = subMenu.IsActive });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateSubMenuRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);
        var result = await _apiClient.PutAsync<object>($"api/v1/SubMenus/{request.Id}", new { id = request.Id, menuId = request.MenuId, name = request.Name, displayName = request.DisplayName, icon = request.Icon, route = request.Route, displayOrder = request.DisplayOrder, isActive = request.IsActive });
        if (result.Success)
        {
            TempData["SuccessMessage"] = "SubMenu updated successfully";
            return RedirectToAction("Index", new { menuId = request.MenuId });
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/SubMenus/{id}");
        if (result.Success)
            TempData["SuccessMessage"] = "SubMenu deleted successfully";
        else
            TempData["Error"] = result.Message;
        return RedirectToAction("Index");
    }
}
