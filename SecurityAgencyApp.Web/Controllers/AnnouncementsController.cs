using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class AnnouncementsController : Controller
{
    private readonly IApiClient _apiClient;

    public AnnouncementsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? category = null, bool? isPinned = null, bool includeInactive = false, string? sortBy = null, string? sortDirection = "desc")
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["includeInactive"] = includeInactive.ToString().ToLowerInvariant()
        };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (!string.IsNullOrEmpty(category)) query["category"] = category;
        if (isPinned.HasValue) query["isPinned"] = isPinned.Value.ToString().ToLowerInvariant();
        if (!string.IsNullOrEmpty(sortBy)) query["sortBy"] = sortBy;
        if (!string.IsNullOrEmpty(sortDirection)) query["sortDirection"] = sortDirection;

        var result = await _apiClient.GetAsync<AnnouncementListResponse>("api/v1/Announcements", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new AnnouncementListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateAnnouncementRequest { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAnnouncementRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);
        var body = new { title = request.Title, content = request.Content, category = request.Category, isPinned = request.IsPinned, isActive = request.IsActive };
        var result = await _apiClient.PostAsync<Guid>("api/v1/Announcements", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Announcement created successfully.";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message ?? "Failed to create announcement.");
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<AnnouncementItemDto>($"api/v1/Announcements/{id}");
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = "Announcement not found.";
            return RedirectToAction(nameof(Index));
        }
        var item = result.Data;
        return View(new UpdateAnnouncementRequest
        {
            Id = item.Id,
            Title = item.Title,
            Content = item.Content,
            Category = item.Category,
            IsPinned = item.IsPinned,
            IsActive = item.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateAnnouncementRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);
        var body = new { title = request.Title, content = request.Content, category = request.Category, isPinned = request.IsPinned, isActive = request.IsActive };
        var result = await _apiClient.PutAsync<object>($"api/v1/Announcements/{request.Id}", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Announcement updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message ?? "Failed to update announcement.");
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/Announcements/{id}");
        if (result.Success)
            TempData["SuccessMessage"] = "Announcement deleted.";
        else
            TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
