using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class SitesController : Controller
{
    private readonly IApiClient _apiClient;

    public SitesController(IApiClient apiClient)
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
        var roles = HttpContext.Session.GetString("Roles") ?? "";
        if (roles.Contains("SUPERVISOR", StringComparison.OrdinalIgnoreCase))
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (Guid.TryParse(userIdStr, out var currentUserId))
                query["supervisorId"] = currentUserId.ToString();
        }

        var result = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new SiteListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadSupervisorsIntoViewBag(null);
        return View(new CreateSiteRequest { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSiteRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _apiClient.PostAsync<Guid>("api/v1/Sites", new
        {
            siteCode = request.SiteCode,
            siteName = request.SiteName,
            clientName = request.ClientName,
            address = request.Address,
            city = request.City,
            state = request.State,
            pinCode = request.PinCode,
            contactPerson = request.ContactPerson ?? "",
            contactPhone = request.ContactPhone ?? "",
            contactEmail = request.ContactEmail,
            isActive = request.IsActive,
            latitude = request.Latitude,
            longitude = request.Longitude,
            geofenceRadiusMeters = request.GeofenceRadiusMeters,
            supervisorIds = request.SupervisorIds ?? new List<Guid>()
        });

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Site created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<SiteDto>($"api/v1/Sites/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();

        var d = result.Data;
        await LoadSupervisorsIntoViewBag(d.SupervisorIds);
        return View(new UpdateSiteRequest
        {
            Id = d.Id,
            SiteCode = d.SiteCode,
            SiteName = d.SiteName,
            ClientName = d.ClientName,
            Address = d.Address,
            City = d.City,
            State = d.State,
            PinCode = d.PinCode,
            ContactPerson = d.ContactPerson,
            ContactPhone = d.ContactPhone,
            ContactEmail = d.ContactEmail,
            IsActive = d.IsActive,
            Latitude = d.Latitude,
            Longitude = d.Longitude,
            GeofenceRadiusMeters = d.GeofenceRadiusMeters,
            SupervisorIds = d.SupervisorIds ?? new List<Guid>()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateSiteRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _apiClient.PutAsync<object>($"api/v1/Sites/{request.Id}", new
        {
            id = request.Id,
            siteCode = request.SiteCode,
            siteName = request.SiteName,
            clientName = request.ClientName,
            address = request.Address,
            city = request.City,
            state = request.State,
            pinCode = request.PinCode,
            contactPerson = request.ContactPerson ?? "",
            contactPhone = request.ContactPhone ?? "",
            contactEmail = request.ContactEmail,
            isActive = request.IsActive,
            latitude = request.Latitude,
            longitude = request.Longitude,
            geofenceRadiusMeters = request.GeofenceRadiusMeters,
            supervisorIds = request.SupervisorIds ?? new List<Guid>()
        });

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Site updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/Sites/{id}");
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Site deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSupervisorsIntoViewBag(IList<Guid>? selectedIds = null)
    {
        var res = await _apiClient.GetAsync<UserListResponse>("api/v1/Supervisors", new Dictionary<string, string?> { ["pageSize"] = "500", ["isActive"] = "true" });
        var list = res.Success && res.Data?.Items != null ? res.Data.Items : new List<UserItemDto>();
        var items = list.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = string.IsNullOrWhiteSpace($"{s.FirstName} {s.LastName}".Trim()) ? s.UserName : $"{s.FirstName} {s.LastName}".Trim()
        }).ToList();
        ViewBag.Supervisors = items;
    }
}
