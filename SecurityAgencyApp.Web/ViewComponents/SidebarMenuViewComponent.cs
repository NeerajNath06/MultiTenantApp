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
        var query = new Dictionary<string, string?>
        {
            ["includeInactive"] = "false",
            ["pageSize"] = "100"
        };
        var result = await _apiClient.GetAsync<MenuListResponse>("api/v1/Menus", query);
        var items = result.Success && result.Data?.Items != null
            ? result.Data.Items.OrderBy(m => m.DisplayOrder).ToList()
            : new List<MenuDto>();
        return View(items);
    }
}
