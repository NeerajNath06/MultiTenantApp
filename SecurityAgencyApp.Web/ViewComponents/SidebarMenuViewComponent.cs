using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.ViewComponents;

/// <summary>
/// Loads sidebar menu from API (database). Web lightweight – menus in DB, not hardcoded.
/// </summary>
public class SidebarMenuViewComponent : ViewComponent
{
    private readonly IApiClient _apiClient;

    public SidebarMenuViewComponent(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Role-based menus from DB – no hardcoding. Uses for-current-user so only allowed menus show.
        var result = await _apiClient.GetAsync<MenuListResponse>("api/v1/Menus/for-current-user");
        var items = result.Success && result.Data?.Items != null
            ? result.Data.Items.OrderBy(m => m.DisplayOrder).ToList()
            : new List<MenuDto>();
        return View(items);
    }
}
