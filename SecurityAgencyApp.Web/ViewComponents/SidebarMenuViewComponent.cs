using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Model.Api;
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
        var session = HttpContext?.Session;
        var hasUserSession = session != null && !string.IsNullOrEmpty(session.GetString("UserId"));
        var hasToken = session != null && !string.IsNullOrEmpty(session.GetString("AccessToken"));

        List<MenuDto> items = new();
        if (hasToken)
        {
            var result = await _apiClient.GetAsync<MenuListResponse>("api/v1/Menus/for-current-user");
            if (result.Success && result.Data?.Items != null)
            {
                items = result.Data.Items.OrderBy(m => m.DisplayOrder).ToList();
            }
        }

        // Resilience: if API is unavailable/misconfigured, still render a usable sidebar.
        if (items.Count == 0)
        {
            items = GetFallbackMenus(hasUserSession);
        }

        return View(items);
    }

    private static List<MenuDto> GetFallbackMenus(bool isAuthenticated)
    {
        if (!isAuthenticated)
        {
            return new List<MenuDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Auth",
                    DisplayName = "Account",
                    Icon = "fas fa-user-circle",
                    Route = "#",
                    DisplayOrder = 1,
                    IsActive = true,
                    SubMenus = new List<SubMenuItemDto>
                    {
                        new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Auth", Name = "Login", DisplayName = "Login", Icon = "fas fa-sign-in-alt", Route = "Auth/Login", DisplayOrder = 1, IsActive = true },
                        new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Auth", Name = "Register", DisplayName = "Register", Icon = "fas fa-building", Route = "Auth/Register", DisplayOrder = 2, IsActive = true }
                    }
                }
            };
        }

        return new List<MenuDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Home",
                DisplayName = "Dashboard",
                Icon = "fas fa-home",
                Route = "Home/Index",
                DisplayOrder = 1,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Operations",
                DisplayName = "Operations",
                Icon = "fas fa-briefcase",
                Route = "#",
                DisplayOrder = 2,
                IsActive = true,
                SubMenus = new List<SubMenuItemDto>
                {
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Operations", Name = "GuardAssignments", DisplayName = "Guard Assignments", Icon = "fas fa-tasks", Route = "GuardAssignments/Index", DisplayOrder = 1, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Operations", Name = "Attendance", DisplayName = "Attendance", Icon = "fas fa-calendar-check", Route = "Attendance/Index", DisplayOrder = 2, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Operations", Name = "Branches", DisplayName = "Branches", Icon = "fas fa-code-branch", Route = "Branches/Index", DisplayOrder = 3, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Operations", Name = "SitePosts", DisplayName = "Site Posts", Icon = "fas fa-users-cog", Route = "SitePosts/Index", DisplayOrder = 4, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Operations", Name = "Roster", DisplayName = "Roster", Icon = "fas fa-calendar-alt", Route = "Roster/Index", DisplayOrder = 5, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Operations", Name = "Incidents", DisplayName = "Incidents", Icon = "fas fa-exclamation-triangle", Route = "Incidents/Index", DisplayOrder = 6, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Operations", Name = "Vehicles", DisplayName = "Vehicle Logs", Icon = "fas fa-car", Route = "Vehicles/Index", DisplayOrder = 7, IsActive = true }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Masters",
                DisplayName = "Master Data",
                Icon = "fas fa-database",
                Route = "#",
                DisplayOrder = 3,
                IsActive = true,
                SubMenus = new List<SubMenuItemDto>
                {
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Masters", Name = "Clients", DisplayName = "Clients", Icon = "fas fa-building", Route = "Clients/Index", DisplayOrder = 1, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Masters", Name = "Sites", DisplayName = "Sites", Icon = "fas fa-map-marker-alt", Route = "Sites/Index", DisplayOrder = 2, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Masters", Name = "Shifts", DisplayName = "Shifts", Icon = "fas fa-clock", Route = "Shifts/Index", DisplayOrder = 3, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Masters", Name = "SecurityGuards", DisplayName = "Security Guards", Icon = "fas fa-user-shield", Route = "SecurityGuards/Index", DisplayOrder = 4, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Masters", Name = "Users", DisplayName = "Users", Icon = "fas fa-users", Route = "Users/Index", DisplayOrder = 5, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Masters", Name = "Departments", DisplayName = "Departments", Icon = "fas fa-sitemap", Route = "Departments/Index", DisplayOrder = 6, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Masters", Name = "Designations", DisplayName = "Designations", Icon = "fas fa-id-badge", Route = "Designations/Index", DisplayOrder = 7, IsActive = true }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                DisplayName = "Administration",
                Icon = "fas fa-cogs",
                Route = "#",
                DisplayOrder = 4,
                IsActive = true,
                SubMenus = new List<SubMenuItemDto>
                {
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Admin", Name = "Roles", DisplayName = "Roles", Icon = "fas fa-user-tag", Route = "Roles/Index", DisplayOrder = 1, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Admin", Name = "Menus", DisplayName = "Menus", Icon = "fas fa-list", Route = "Menus/Index", DisplayOrder = 2, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Admin", Name = "SubMenus", DisplayName = "Sub Menus", Icon = "fas fa-list-ul", Route = "SubMenus/Index", DisplayOrder = 3, IsActive = true },
                    new() { Id = Guid.NewGuid(), MenuId = Guid.Empty, MenuName = "Admin", Name = "CompanyProfile", DisplayName = "Company Profile", Icon = "fas fa-building", Route = "CompanyProfile/Index", DisplayOrder = 4, IsActive = true }
                }
            }
        };
    }
}
