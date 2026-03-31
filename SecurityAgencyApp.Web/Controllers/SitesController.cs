using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class SitesController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = "asc")
    {
        var roles = HttpContext.Session.GetString("Roles") ?? "";
        if (roles.Contains("SUPERVISOR", StringComparison.OrdinalIgnoreCase))
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (Guid.TryParse(userIdStr, out var currentUserId))
            {
                ViewBag.SupervisorId = currentUserId;
            }
        }

        return View(new SiteListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateSiteRequest
        {
            IsActive = true,
            Posts = new List<SitePostDto>(),
            SupervisorIds = new List<Guid>(),
            ActiveDeploymentPlan = new SiteDeploymentPlanDto
            {
                EffectiveFrom = DateTime.Today,
                IsActive = true
            }
        });
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateSiteRequest
        {
            Id = id,
            IsActive = true,
            Posts = new List<SitePostDto>(),
            SupervisorIds = new List<Guid>(),
            ActiveDeploymentPlan = new SiteDeploymentPlanDto
            {
                EffectiveFrom = DateTime.Today,
                IsActive = true
            }
        });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.SiteId = id;
        return View();
    }
}
