using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly IApiClient _apiClient;

    public NotificationsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>My notifications (admin/supervisor/guard - whoever is logged in).</summary>
    public async Task<IActionResult> Index(bool? isRead = null, string? type = null, int pageNumber = 1, int pageSize = 20)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            TempData["Error"] = "Session expired. Please login again.";
            return RedirectToAction("Login", "Auth");
        }

        var query = new Dictionary<string, string?>
        {
            ["userId"] = userId.ToString(),
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString()
        };
        if (isRead.HasValue) query["isRead"] = isRead.Value.ToString().ToLowerInvariant();
        if (!string.IsNullOrEmpty(type)) query["type"] = type;

        var result = await _apiClient.GetAsync<NotificationListResponse>("api/v1/Notifications", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new NotificationListResponse());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction(nameof(Index));
        var result = await _apiClient.PatchAsync<object>($"api/v1/Notifications/{id}/read?userId={userIdStr}", null);
        if (result.Success) TempData["SuccessMessage"] = "Marked as read.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction(nameof(Index));
        var result = await _apiClient.PatchAsync<object>($"api/v1/Notifications/read-all?userId={userIdStr}", null);
        if (result.Success) TempData["SuccessMessage"] = "All marked as read.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Send notification to users (guards, supervisors, etc.).</summary>
    public async Task<IActionResult> Send()
    {
        await LoadRecipients();
        return View(new SendNotificationRequest { Type = "Info" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(SendNotificationRequest request)
    {
        if (request?.UserIds == null || request.UserIds.Count == 0)
        {
            ModelState.AddModelError("", "Select at least one recipient.");
            await LoadRecipients();
            return View(request ?? new SendNotificationRequest());
        }
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            ModelState.AddModelError("", "Title is required.");
            await LoadRecipients();
            return View(request);
        }
        var body = new { userIds = request.UserIds, title = request.Title.Trim(), body = (request.Body ?? "").Trim(), type = request.Type ?? "Info" };
        var result = await _apiClient.PostAsync<object>("api/v1/Notifications/send", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Notification sent successfully.";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message ?? "Failed to send.");
        await LoadRecipients();
        return View(request);
    }

    private async Task LoadRecipients()
    {
        var queryAll = new Dictionary<string, string?> { ["pageSize"] = "1000", ["isActive"] = "true" };
        var userResult = await _apiClient.GetAsync<UserListResponse>("api/v1/Users", queryAll);
        var users = userResult.Data?.Items ?? new List<UserItemDto>();
        var list = users.Select(u => new { Value = u.Id.ToString(), Text = $"{u.FirstName} {u.LastName} ({u.UserName})" }).ToList();
        ViewBag.Recipients = new MultiSelectList(list, "Value", "Text");

        var guardResult = await _apiClient.GetAsync<UserListResponse>("api/v1/Users", new Dictionary<string, string?> { ["pageSize"] = "1000", ["isActive"] = "true", ["roleCode"] = "GUARD" });
        var guardUserIds = (guardResult.Data?.Items ?? new List<UserItemDto>()).Select(u => u.Id.ToString()).ToList();
        ViewBag.GuardUserIds = guardUserIds;

        var supervisorResult = await _apiClient.GetAsync<UserListResponse>("api/v1/Users", new Dictionary<string, string?> { ["pageSize"] = "1000", ["isActive"] = "true", ["roleCode"] = "SUPERVISOR" });
        var supervisorUserIds = (supervisorResult.Data?.Items ?? new List<UserItemDto>()).Select(u => u.Id.ToString()).ToList();
        ViewBag.SupervisorUserIds = supervisorUserIds;

        ViewBag.AllUserIds = users.Select(u => u.Id.ToString()).ToList();
    }
}
