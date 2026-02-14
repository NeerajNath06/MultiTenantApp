using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class MenusController : Controller
{
    private readonly IApiClient _apiClient;

    public MenusController(IApiClient apiClient)
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
        var result = await _apiClient.GetAsync<MenuListResponse>("api/v1/Menus", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new MenuListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateMenuRequest { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMenuRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);
        var result = await _apiClient.PostAsync<Guid>("api/v1/Menus", new { name = request.Name, displayName = request.DisplayName, icon = request.Icon, route = request.Route, displayOrder = request.DisplayOrder, isActive = request.IsActive });
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Menu created successfully!";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }
}
