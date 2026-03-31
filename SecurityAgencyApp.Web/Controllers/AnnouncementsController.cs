using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class AnnouncementsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? category = null, bool? isPinned = null, bool includeInactive = false, string? sortBy = null, string? sortDirection = "desc")
    {
        return View(new AnnouncementListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateAnnouncementRequest { IsActive = true });
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateAnnouncementRequest
        {
            Id = id,
            IsActive = true
        });
    }
}
