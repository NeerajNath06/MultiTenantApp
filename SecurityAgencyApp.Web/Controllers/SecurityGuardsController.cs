using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class SecurityGuardsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = "asc", Guid? supervisorId = null)
    {
        Guid? effectiveSupervisorId = supervisorId;
        if (!effectiveSupervisorId.HasValue && string.Equals(HttpContext.Session.GetString("IsSupervisor"), "True", StringComparison.OrdinalIgnoreCase))
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
                effectiveSupervisorId = userId;
        }

        ViewBag.SupervisorId = effectiveSupervisorId;
        return View(new GuardListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateGuardRequest());
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.GuardId = id;
        return View();
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateGuardRequest
        {
            Id = id
        });
    }
}
